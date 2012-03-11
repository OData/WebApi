###!
nav.transitions.js v0.1 - (c) Microsoft Corporation
###

# Pane transitions. Currently assumes availability of XUI. Need to generalise for jQuery.
$ = x$

# Feature detection
features =
    vendor:
        if (/webkit/i).test(navigator.appVersion) then		'webkit'
        else if (/firefox/i).test(navigator.userAgent) then 'Moz'
        else if 'opera' of window then						'O'
        else ''
    isAndroid: (/android/gi).test(navigator.appVersion)

features.useCssTransform = (!features.isAndroid) && (features.vendor + 'Transform' of document.documentElement.style)
features.cssTransformPrefix = "-" + features.vendor.toLowerCase() + "-"
features.transitionEndEvent =
    if features.vendor == 'webkit' then 'webkitTransitionEnd'
    else if features.vendor == 'O' then 'oTransitionEnd'
    else 'transitionend'
    
$.isTouch = 'ontouchstart' of document.documentElement
$.clickOrTouch = if $.isTouch then 'touchstart' else 'click'
features.supportsCssTouchScroll = (typeof document.body.style.webkitOverflowScrolling != "undefined") # Currently only iOS 5 can do native touch scrolling
features.supportsIScroll = (features.vendor == 'webkit' || features.vendor == "Moz")

# Utilities
findFirstChildWithClass = (elem, className) ->
    child = elem.firstChild
    while child
        if $(child).hasClass(className) then return child
        child = child.nextSibling
    return null

oppositeDirection = { left: 'right', right: 'left', top: 'bottom', bottom: 'top' }
oppositeTransition = (transition) ->
    if transition
        for own key of transition
            if $.paneTransitionInverters.hasOwnProperty(key)
                return $.paneTransitionInverters[key](transition[key])
    null

# Implement $.getJSON and $.map like jQuery does
$.getJSON = (url, options) ->
    callback = if typeof options == "function" then options else options.callback
    $(null).xhr(url, {
        method: options.method,
        async: true,
        data: JSON.stringify(options.data),
        headers: options.headers,
        callback: ->
            callback(JSON.parse(this.responseText))
    })

$.map = (items, map) ->
    results = []
    for item in items
        mapped = map(item)
        if mapped != undefined
            results.push(mapped)
    results

# XUI extensions
$.fn.togglePane = (show) ->
    @each ->
        # Can't just toggle display:block/none, as that resets any scroll positions within the pane
        # Can't just toggle visibility:visible/hidden either, as Safari iOS still sends events (e.g., taps) into the element
        # So, just move it very far away
        if show
            @style.display = 'block'
            if @isOffScreen
                @isOffScreen = false
                @style.top = ''
                @style.bottom = ''
        else
            @isOffScreen = true
            @style.top = '-10000px'
            @style.bottom = '10000px'
        this

$.fn.afterNextTransition = (callback) ->
    @each ->
        elem = this
        handlerWrapper = () ->
            callback.apply(this, arguments)
            elem.removeEventListener(features.transitionEndEvent, handlerWrapper)
        elem.addEventListener(features.transitionEndEvent, handlerWrapper);

$.fn.animateTranslation = (finalPos, transition, callback) ->
    callback = callback || () -> { }
    @each ->		
        $this = $(this)
        if features.useCssTransform
            transform = {}
            transform[features.cssTransformPrefix + "transform"] = "translate(" + finalPos.left + ", " + finalPos.top + ")"
            transform[features.cssTransformPrefix + "transition"] = if transition then features.cssTransformPrefix + "transform 250ms ease-out" else null

            if transition
                $this.afterNextTransition(callback)

            $this.css(transform)
            if !transition
                callback()
        else
            if transition
                $this.tween(finalPos, callback)
            else
                $this.css(finalPos)
                callback()

$.fn.setPanePosition = (position, transition, callback) ->
    callback = callback || () -> { }
    
    @each ->
        $this = $(this).togglePane(true)
        x = 0
        y = 0
        width = @parentNode.offsetWidth
        height = @parentNode.offsetHeight

        switch position
            when 'right'  then x = width
            when 'left'   then x = -1 * width
            when 'top'    then y = -1 * height
            when 'bottom' then y = height

        finalPos = { left: x + 'px', right: (-1 * x) + 'px', top: y + 'px', bottom: (-1 * y) + 'px' }
        $this.animateTranslation(finalPos, transition, callback)

$.fn.slidePane = (options) ->
    @each ->
        $this = $(this)
        afterSlide = ->
            if options.to then $this.togglePane(false)
            if options.callback then options.callback()
        $this.setPanePosition(options.from, null)
                .setPanePosition(options.to, true, afterSlide)

$.fn.showPane = (options) ->
    options = options || {}
    @each ->
        activePane = findFirstChildWithClass(this.parentNode, "active")
        if activePane != this # Not already shown
            $(this).has(".scroll-y.autoscroll").touchScroll({ hScroll: false })
            $(this).has(".scroll-x.autoscroll").touchScroll({ yScroll: false })

            # Find and invoke the requested transition
            transitionToUse = 'default'
            for own transitionKey of $.paneTransitions
                if options.hasOwnProperty(transitionKey)
                    transitionToUse = transitionKey
                    break
            $.paneTransitions[transitionKey](this, activePane, options[transitionKey])

            # Keep track of which pane is active			
            $(this).addClass("active")
            if activePane
                $(activePane).removeClass("active")

$.fn.showBySlidingParent = (options) ->
    @each ->                   
        targetPaneOffsetLeft = parseInt(@style.left) || 0
        targetPaneOffsetTop = parseInt(@style.top) || 0
        finalPos =
            left: (-1*targetPaneOffsetLeft) + '%'
            right: targetPaneOffsetLeft + '%'
            top: (-1*targetPaneOffsetTop) + '%'
            bottom: targetPaneOffsetTop + '%'
        $(this).css({ display: 'block' })
        $(@parentNode).css({ 'overflow': 'visible' }).animateTranslation(finalPos, options.animate != false)
        this

$.fn.touchScroll = (options) ->
    if (!features.supportsCssTouchScroll) && features.supportsIScroll
        @each -> 
            if !@hasIScroll
                @hasIScroll = new iScroll(this, options)
            doRefresh = => @hasIScroll.refresh()
            setTimeout(doRefresh, 0) 
            this
    this

$.fn.clickOrTouch = (handler) ->
    @on($.clickOrTouch, handler)

# Create missing event shortcuts for IE version of XUI
for eventName in ['click']
    if (!$.fn[eventName])
        $.fn[eventName] = (handler) -> @on(eventName, handler)

# Transitions
$.paneTransitions =
    slideFrom: (incomingPane, outgoingPane, options) ->
        $(incomingPane).slidePane({ from: options })
        if outgoingPane
            $(outgoingPane).slidePane({ to: oppositeDirection[options] })

    coverFrom: (incomingPane, outgoingPane, options) ->
        outgoingZIndex = outgoingPane?.style.zIndex || 0
        $(incomingPane).css({ zIndex: outgoingZIndex + 1 })
                        .slidePane({
                            from: options,
                            callback: () -> $(outgoingPane).togglePane(false)
                        })
    
    uncoverTo: (incomingPane, outgoingPane, options) -> 
        incomingZIndex = incomingPane.style.zIndex || 0;
        $(incomingPane).togglePane(true).setPanePosition()
        $(outgoingPane).css({ zIndex: incomingZIndex + 1 }).slidePane({ to: options })
    
    default: (incomingPane, outgoingPane, options) ->
        # No transition - just show instantly, and hide the previously active pane
        $(incomingPane).togglePane(true).setPanePosition()
        $(outgoingPane).togglePane(false)

$.paneTransitionInverters =
    slideFrom: (direction) -> { slideFrom: oppositeDirection[direction] }
    coverFrom: (direction) -> { uncoverTo: direction }
    uncoverTo: (direction) -> { coverFrom: direction }

# Hook into nav.js so you can easily animate transitions on navigation
window.NavHistory.animatePane = (elementId, navInfo) ->
    transition = navInfo.transition || (if !navInfo.isFirst then { slideFrom: 'right' } else null)
    if navInfo.isBack
        transition = oppositeTransition(transition)
    x$('#' + elementId).showPane(transition)

window.NavHistory.slideParent = (elementId, navInfo) ->
    x$('#' + elementId).showBySlidingParent({ animate: !navInfo.isFirst })