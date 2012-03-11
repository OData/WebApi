###!
nav.js v0.1 - (c) Microsoft Corporation
###

# Small helper function for creating jQuery-style read/write function wrappers around an underlying value
readWriteValue = (initialValue) ->
    currentValue = initialValue
    return () -> 
        if arguments.length > 0 then currentValue = arguments[0]
        currentValue

class window.NavHistory
    constructor: (opts) ->
        @options = opts || {}
        @options.params = @_extend({}, @options.params, @_asString) # Ensure all defaults are strings
        @isLinkedToUrl = false

        # If KO is referenced, @position and @entries will be observable. Otherwise, use plain JS objects.
        # You can override this via @options.ko if you want.
        useKo = if 'ko' of @options then @options.ko else ko?.observable?
        @position = if useKo then ko.observable(-1) else readWriteValue(-1)
        @entries  = if useKo then ko.observableArray([]) else []

    length: => @_entriesArray().length
    relative: (offset) => @_entriesArray()[@position() + offset] || {}
    current: => @relative(0)
    params: => @current().params || {}
    loadedData: => @current().loadedData
    back: => if @position() > 0 then @navigateAll(@relative(-1).params)
    forward: => if @position() < @length() - 1 then @navigateAll(@relative(1).params)
    _entriesArray: => if typeof @entries == 'function' then @entries() else @entries

    initialize: (opts) ->
        if opts?.linkToUrl
            @_linkToUrl()
        else
            @navigateAll(opts?.params || {})
        return this

    navigate: (newParams, opts) =>
        # Retain any params not specified
        newParamsPlusCurrent = @_extend(@_extend({}, @params()), newParams)
        @navigateAll(newParamsPlusCurrent, opts)        

    navigateAll: (newParams, opts) =>
        newParams = @_normalizeParams(newParams)
        isBack = false
        isForward = false
        isNoChange = false
        transition = opts?.transition
        navEntry = null        

        if @length() && @_propsAreEqual(newParams, @params())
            # We're already there. No need to create a new nav entry.
            if opts?.force
                isNoChange = true
                navEntry = @current()
            else
                # In the absence of a "force" directive, we don't even need to navigate at all
                return

        else if @_propsAreEqual(newParams, @relative(-1)?.params)
            # It's a "back" - reuse existing transition if no new one was given
            isBack = true
            transition = transition || @current().savedTransition
            navEntry = @relative(-1)

        else if @_propsAreEqual(newParams, @relative(1)?.params)
            # It's a "forward" - reuse existing transition if no new one was given
            isForward = true
            navEntry = @relative(1)
            transition = transition || @current().savedTransition

        else
            # Entirely new navigation - create new entry
            navEntry = { params: newParams, navEntryId: "navEntry_" + @_getUniqueSequenceValue() }
        
        # Extra param for beforeNavigate/onNavigate callbacks
        # Note that navInfo.transition is what will be used during the current navigation (regardless of direction)
        # whereas navEntry.forwardTransition is what we're storing in case we need to reuse transitions in the future
        navInfo = { isFirst: @length() == 0, isBack: isBack, isForward: isForward, transition: transition }

        beforeNavigateCallback = () =>
            if isBack
                # Consider also doing a History.back() here. This feels more natural if there's only a single NavHistory instance,
                # because then programmatic back/forwards is the same as using the browser controls. But if you have multiple NavHistory
                # instances, you can't be sure that History.back() will take you to the same place that this instance has logged.
                @position(@position() - 1)				
            else if isForward
                # As above comment, except with History.forwards
                @position(@position() + 1)
            else if !isNoChange
                # Clear "forward" items, and add new entry
                deleteCount = @length() - @position() - 1
                @entries().splice(@position() + 1, deleteCount, navEntry)

                # Move to it, possibly by removing the first entry if we've exceeded capacity
                if @options.maxEntries && @length() > @options.maxEntries
                    @entries.shift() # This will notify subscribers to @entries, if it's observable
                else
                    @position(@position() + 1)
                    # Notify subscribers to @entries, if it's observable
                    if typeof @entries.valueHasMutated == 'function'
                        @entries.valueHasMutated()

            if !isBack && navInfo.transition
                @current().savedTransition = navInfo.transition

            # Consider only using pushState for totally new navigations (not isBack and not isForwards), and using History.back()
            # and History.forwards() as in above comments.
            if @isLinkedToUrl && (opts?.updateUrl != false) && !isNoChange
                updatedQueryString = @_getUpdatedQueryString(@params())
                window.NavHistory.historyProvider.pushState({ url: updatedQueryString })
            
            if @options.onNavigate
                @options.onNavigate.call(this, @current(), navInfo)

        if !@options.beforeNavigate
            beforeNavigateCallback()
        else
            threadLoadToken = @objectLoadToken = {}
            @options.beforeNavigate.call(this, navEntry, navInfo, ((loadedData) =>
                # Ignore the callback unless threadLoadToken still matches. Avoids race conditions.
                if threadLoadToken == @objectLoadToken
                    if loadedData != undefined
                        navEntry.loadedData = loadedData
                    beforeNavigateCallback()
            ))
        this

    _asString: (val) -> if val == null || val == undefined then "" else val.toString()

    _extend: (target, source, mapFunction) ->
        for own key, value of source
            target[key] = if mapFunction then mapFunction(value) else value
        target

    _normalizeParams: (params) ->
        # Normalized params are purely strings, and contain a value for every default
        defaults = @options.params || {}
        @_extend(@_extend({}, defaults), params || {}, @_asString)

    _propsAreEqual: (obj1, obj2) ->
        if !(obj1 && obj2)
            return obj1 == obj2
        for own obj1key, obj1value of obj1
            if obj2[obj1key] != obj1value then return false
        for own obj2key, obj2value of obj2
            if obj1[obj2key] != obj2value then return false
        true

    _parseQueryString: (url) ->
        if url.indexOf('?') < 0 then return {}
        query = url.substring(url.lastIndexOf('?') + 1)
        result = {}
        for pair in query.split("&")
            tokens = pair.split("=")
            if (tokens.length == 2)
                result[tokens[0]] = decodeURIComponent(tokens[1])
        result

    _formatQueryString: (params) ->
        formattedUrl = '?'
        for own key, value of params
            if formattedUrl != '?'
                formattedUrl += '&'
            formattedUrl += key + '=' + encodeURIComponent(value)
        formattedUrl

    _getUpdatedQueryString: (params) ->
        # Take the existing query string...
        allUrlParams = @_parseQueryString(window.NavHistory.historyProvider.getState().url)

        # ... update the params based on the supplied arg (removing any that correspond to our default)
        for own key, defaultValue of @options.params
            suppliedValue = params[key]
            if suppliedValue == defaultValue
                delete allUrlParams[key]
            else
                allUrlParams[key] = suppliedValue

        # ... and return the resulting querystring
        @_formatQueryString(allUrlParams)	

    _getUniqueSequenceValue: () ->
        NavHistory._sequence = NavHistory._sequence || 0
        (NavHistory._sequence++).toString()

    _linkToUrl: ->
        @isLinkedToUrl = true
        onStateChange = =>
            # Get the subset of URL params that applies to this NavHistory instance
            applicableParams = {}
            allUrlParams = @_parseQueryString(window.NavHistory.historyProvider.getState().url)
            defaults = @options.params || {}
            for own key, value of allUrlParams when defaults.hasOwnProperty(key)
                applicableParams[key] = value

            # ... and navigate to the new params
            @navigateAll(applicableParams, { updateUrl: false })

        # Perform initial navigation (loads state from the requested URL)		
        onStateChange()

        # Respond to future URL changes too
        window.NavHistory.historyProvider.onStateChange(onStateChange)

# Default history provider is history.js
window.NavHistory.historyProvider = 
    onStateChange: (handler) -> History.Adapter.bind(window, 'statechange', handler)
    pushState: (data) -> History.pushState(null, null, data.url)
    getState: -> History.getState()
    back: -> History.back()

# Helper to display a specific element, simultaneously hiding its siblings. Useful for toggling panes, tabs, etc.
# Does not require any DOM library.
window.NavHistory.showPane = (elementId, navInfo) ->
    elemToShow = document.getElementById(elementId)
    if elemToShow
        for sibling in elemToShow.parentNode.childNodes when sibling.nodeType == 1
            sibling.style.display = 'none'
        elemToShow.style.display = 'block'