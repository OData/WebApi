// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser;

namespace System.Web.Razor.Text
{
    public class SourceLocationTracker
    {
        private int _absoluteIndex = 0;
        private int _characterIndex = 0;
        private int _lineIndex = 0;
        private SourceLocation _currentLocation;

        public SourceLocationTracker()
            : this(SourceLocation.Zero)
        {
        }

        public SourceLocationTracker(SourceLocation currentLocation)
        {
            CurrentLocation = currentLocation;

            UpdateInternalState();
        }

        public SourceLocation CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                if (_currentLocation != value)
                {
                    _currentLocation = value;
                    UpdateInternalState();
                }
            }
        }

        public void UpdateLocation(char characterRead, char nextCharacter)
        {
            _absoluteIndex++;

            if (ParserHelpers.IsNewLine(characterRead) && (characterRead != '\r' || nextCharacter != '\n'))
            {
                _lineIndex++;
                _characterIndex = 0;
            }
            else
            {
                _characterIndex++;
            }

            UpdateLocation();
        }

        public SourceLocationTracker UpdateLocation(string content)
        {
            for (int i = 0; i < content.Length; i++)
            {
                char nextCharacter = '\0';
                if (i < content.Length - 1)
                {
                    nextCharacter = content[i + 1];
                }
                UpdateLocation(content[i], nextCharacter);
            }
            return this;
        }

        private void UpdateInternalState()
        {
            _absoluteIndex = CurrentLocation.AbsoluteIndex;
            _characterIndex = CurrentLocation.CharacterIndex;
            _lineIndex = CurrentLocation.LineIndex;
        }

        private void UpdateLocation()
        {
            CurrentLocation = new SourceLocation(_absoluteIndex, _lineIndex, _characterIndex);
        }

        public static SourceLocation CalculateNewLocation(SourceLocation lastPosition, string newContent)
        {
            return new SourceLocationTracker(lastPosition).UpdateLocation(newContent).CurrentLocation;
        }
    }
}
