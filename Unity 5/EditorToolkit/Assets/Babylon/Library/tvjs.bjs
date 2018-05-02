// #      The MIT License (MIT)
// #
// #      Copyright (c) 2016 Microsoft. All rights reserved.
// #
// #      Permission is hereby granted, free of charge, to any person obtaining a copy
// #      of this software and associated documentation files (the "Software"), to deal
// #      in the Software without restriction, including without limitation the rights
// #      to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// #      copies of the Software, and to permit persons to whom the Software is
// #      furnished to do so, subject to the following conditions:
// #
// #      The above copyright notice and this permission notice shall be included in
// #      all copies or substantial portions of the Software.
// #
// #      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// #      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// #      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// #      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// #      LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// #      OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// #      THE SOFTWARE.

//High Level
// -Provides direct APIs to navigate focus to each cardinal direction relative
//  to the current focused element within a specified container
// -Directly listens to configurable key inputs and invokes navigation
//  automatically
// -Coordinates with XYFocus implementations within iframes for seamless focus
//  navigation into and out of an iframe

//Navigation Algorithm
// -Navigation API is called with a given direction
// -Source element is established, usually the current focused element
// -All elements within the focus root (document.body by default) are gathered
//      -Each element's distance from the source element is measured
//      -Each element is assigned a score value which takes the following into
//       account, in order of importance:
//          -The relative size and alignment difference between the element
//           and history rectangle
//          -The distance between the element and source element along the
//           main axis
//          -The relative size and alignment difference between the element
//           and source element
//          -The distance the element and source element  along the co- axis
// -The highest scoring element is resolved which is the candidate for navigation
// -NOTE: a more in -depth summary of this algorithm can be found in the actual
//        source code which has been well- maintained as the code evolved

//Customizations
// The focus request can be customized by the following:
//   -Specific source rectangle/ element, algorithm calculates from this
//    rectangle instead of the current focused element
//   -A focus root, only elements within the focus root are considered as
//    potential targets
//   -A history rectangle, which heavily favors potential target elements that
//    are aligned and of similar size to the history rectangle
//   -Any focusable element can be also annotated with a
//    "data-tv-focus='{ left: "#myDiv" }'" attribute.This attribute is referred
//    to as an XYFocus override.This allows you to directly control where focus
//    should move from this element, given the requested direction.

//Automatic Focus
// The XYFocus module can be configured to listen to specific hotkeys via
// XYFocus.keyCodeMap.When a mapped key input is detected, it will automatically
// invoke the above navigation algorithm.By default, the Xbox DPad and left thumbstick
// keys are mapped.These can be cleared and new keys(e.g.arrow keys, WASD) can be added.

//Considerations when authoring new controls
// When authoring a new control, you may just let XYFocus handle all your focus navigation if the
// focus navigation story is trivial.

//Dealing with edge cases
// Rule of thumb: Ignore it! One important thing to accept is that XYFocus is a heuristic-it is
// not a general focus management solution.It works very well in orthogonal, grid - like layouts
// and will fail utterly outside of them.
//	-There are numerous layouts that can break the XYFocus' heuristic for determining the next
//   focusable element.Most of these are unnatural and contrived layouts where buttons are
//   purposely misaligned and overlapping.Historically, we have ignored these issues.
//  -If a valid edge case is found, we handle it on a case-by -case basis.In most cases,
//   leveraging the XYFocus override is good enough.
//	-If anything more fundamental with the heuristics are found (which has not happened since
//   XYFocus was handed to WinJS by Xbox), consider tweaking the scoring constants - this is the
//   most EXTREME case.
//  -Another category of edge cases revolves around history focus.These have also been historically
//   ignored as no real app has produced any valid layout that triggers these issues.
// One common example here is that you could have a list of buttons so long that the score from
// the primary axis trumps the history score, however, you'd need 10,000s of pixels of consecutive
// buttons which is unrealistic as a focus movement likely will trigger a scroll which invalidates history.

(function () {
    "use strict";

    var CrossDomainMessageConstants = {
        messageDataProperty: "msWinJSXYFocusControlMessage",
        register: "register",
        unregister: "unregister",
        dFocusEnter: "dFocusEnter",
        dFocusExit: "dFocusExit"
    };
    var DirectionNames = {
        left: "left",
        right: "right",
        up: "up",
        down: "down"
    };
    var EventNames = {
        focusChanging: "focuschanging",
        focusChanged: "focuschanged"
    };
    var FocusableTagNames = [
        "A",
        "BUTTON",
        "IFRAME",
        "INPUT",
        "SELECT",
        "TEXTAREA",
        "X-MS-WEBVIEW"
    ];
    var FocusableSelectors = [];

    // These factors can be tweaked to adjust which elements are favored by the focus algorithm
    var ScoringConstants = {
        primaryAxisDistanceWeight: 30,
        secondaryAxisDistanceWeight: 20,
        percentInHistoryShadowWeight: 100000
    };

    /**
        * Gets the mapping object that maps keycodes to XYFocus actions.
    **/
    var _keyCodeMap = {
        left: [],
        right: [],
        up: [],
        down: [],
        accept: []
    };

    var _enabled = true;
    /**
        * Gets or sets the focus root when invoking XYFocus APIs.
    **/
    var _focusRoot;

    function _findNextFocusElement(direction, options) {
        var result = _findNextFocusElementInternal(direction, options);
        return result ? result.target : null;
    }
    function _moveFocus(direction, options) {
        var result = _findNextFocusElement(direction, options);
        if (result) {
            var previousFocusElement = document.activeElement;
            if (_trySetFocus(result, -1)) {
                eventSrc.dispatchEvent(EventNames.focusChanged, { previousFocusElement: previousFocusElement, keyCode: -1 });
                return result;
            }
        }
        return null;
    }
    function _mergeAll(list) {
        // Merge a list of objects together
        var o = {};
        list.forEach(function (part) {
            Object.keys(part).forEach(function (k) {
                o[k] = part[k];
            });
        });
        return o;
    };
    function _createEventProperty(name) {
        var eventPropStateName = "_on" + name + "state";

        return {
            get: function () {
                var state = this[eventPropStateName];
                return state && state.userHandler;
            },
            set: function (handler) {
                var state = this[eventPropStateName];
                if (handler) {
                    if (!state) {
                        state = { wrapper: function (evt) { return state.userHandler(evt); }, userHandler: handler };
                        Object.defineProperty(this, eventPropStateName, { value: state, enumerable: false, writable: true, configurable: true });
                        this.addEventListener(name, state.wrapper, false);
                    }
                    state.userHandler = handler;
                } else if (state) {
                    this.removeEventListener(name, state.wrapper, false);
                    this[eventPropStateName] = null;
                }
            },
            enumerable: true
        };
    };
    // Privates
    var _lastTarget;
    var _cachedLastTargetRect;
    var _historyRect;
    /**
        * Executes XYFocus algorithm with the given parameters. Returns true if focus was moved, false otherwise.
        * @param direction The direction to move focus.
        * @param keyCode The key code of the pressed key.
        * @param referenceRect (optional) A rectangle to use as the source coordinates for finding the next focusable element.
        * @param dontExit (optional) Indicates whether this focus request is allowed to propagate to its parent if we are in iframe.
    **/
    function _xyFocus(direction, keyCode, referenceRect, dontExit) {
        // If focus has moved since the last XYFocus movement, scrolling occured, or an explicit
        // reference rectangle was given to us, then we invalidate the history rectangle.
        if (referenceRect || document.activeElement !== _lastTarget) {
            _historyRect = null;
            _lastTarget = null;
            _cachedLastTargetRect = null;
        }
        else if (_lastTarget && _cachedLastTargetRect) {
            var lastTargetRect = _toIRect(_lastTarget.getBoundingClientRect());
            // Sometimes the bounds calculation for top is off by 1 even though the element has not moved.
            if (lastTargetRect.left !== _cachedLastTargetRect.left || Math.abs(lastTargetRect.top - _cachedLastTargetRect.top) > 1) {
                _cachedLastTargetRect = lastTargetRect;
            }
        }
        var activeElement = document.activeElement;
        var lastTarget = _lastTarget;
        var result = _findNextFocusElementInternal(direction, {
            focusRoot: _focusRoot,
            historyRect: _historyRect,
            referenceElement: _lastTarget,
            referenceRect: referenceRect
        });
        if (result && _trySetFocus(result.target, keyCode)) {
            // A focus target was found
            updateHistoryRect(direction, result);
            _lastTarget = result.target;
            _cachedLastTargetRect = result.targetRect;
            if (result.target.tagName === "IFRAME") {
                var targetIframe = result.target;
                if (IFrameHelper.isXYFocusEnabled(targetIframe)) {
                    // If we successfully moved focus and the new focused item is an IFRAME, then we need to notify it
                    // Note on coordinates: When signaling enter, DO transform the coordinates into the child frame's coordinate system.
                    var refRect = _toIRect({
                        left: result.referenceRect.left - result.targetRect.left,
                        top: result.referenceRect.top - result.targetRect.top,
                        width: result.referenceRect.width,
                        height: result.referenceRect.height
                    });
                    var message = {};
                    message[CrossDomainMessageConstants.messageDataProperty] = {
                        type: CrossDomainMessageConstants.dFocusEnter,
                        direction: direction,
                        referenceRect: refRect
                    };
                    // postMessage API is safe even in cross-domain scenarios.
                    targetIframe.contentWindow.postMessage(message, "*");
                }
            }
            else if (typeof result.target.navigateFocus === "function") {
                // No need to adjust coordinate space of result.referenceRect. navigateFocus/departFocus and corresponding
                // events handle the coordinate space translation.
                result.target.navigateFocus(direction, WebViewHelper.refRectToNavigateFocusRect(result.referenceRect));
            }
            eventSrc.dispatchEvent(EventNames.focusChanged, { previousFocusElement: activeElement, keyCode: keyCode });
            return true;
        }
        else {
            // No focus target was found; if we are inside an IFRAME or webview and focus is allowed to propagate out, notify the parent that focus is exiting
            // Note on coordinates: When signaling exit, do NOT transform the coordinates into the parent's coordinate system.
            if ((!dontExit && top !== window) || typeof window.departFocus === "function") {
                var refRect = referenceRect;
                if (!refRect) {
                    refRect = document.activeElement ? _toIRect(document.activeElement.getBoundingClientRect()) : _defaultRect();
                }
                if (top === window && typeof window.departFocus === "function") {
                    document.activeElement.blur();
                    departFocus(direction, WebViewHelper.refRectToNavigateFocusRect(refRect));
                }
                else {
                    var message = {};
                    message[CrossDomainMessageConstants.messageDataProperty] = {
                        type: CrossDomainMessageConstants.dFocusExit,
                        direction: direction,
                        referenceRect: refRect
                    };
                    // postMessage API is safe even in cross-domain scenarios.
                    parent.postMessage(message, "*");
                }
                return true;
            }
        }
        return false;
        // Nested Helpers
        function updateHistoryRect(direction, result) {
            var newHistoryRect = _defaultRect();
            // It's possible to get into a situation where the target element has no overlap with the reference edge.
            //
            //..╔══════════════╗..........................
            //..║   reference  ║..........................
            //..╚══════════════╝..........................
            //.....................╔═══════════════════╗..
            //.....................║                   ║..
            //.....................║       target      ║..
            //.....................║                   ║..
            //.....................╚═══════════════════╝..
            //
            // If that is the case, we need to reset the coordinates to the edge of the target element.
            if (direction === DirectionNames.left || direction === DirectionNames.right) {
                newHistoryRect.top = Math.max(result.targetRect.top, result.referenceRect.top, _historyRect ? _historyRect.top : Number.MIN_VALUE);
                newHistoryRect.bottom = Math.min(result.targetRect.bottom, result.referenceRect.bottom, _historyRect ? _historyRect.bottom : Number.MAX_VALUE);
                if (newHistoryRect.bottom <= newHistoryRect.top) {
                    newHistoryRect.top = result.targetRect.top;
                    newHistoryRect.bottom = result.targetRect.bottom;
                }
                newHistoryRect.height = newHistoryRect.bottom - newHistoryRect.top;
                newHistoryRect.width = Number.MAX_VALUE;
                newHistoryRect.left = Number.MIN_VALUE;
                newHistoryRect.right = Number.MAX_VALUE;
            }
            else {
                newHistoryRect.left = Math.max(result.targetRect.left, result.referenceRect.left, _historyRect ? _historyRect.left : Number.MIN_VALUE);
                newHistoryRect.right = Math.min(result.targetRect.right, result.referenceRect.right, _historyRect ? _historyRect.right : Number.MAX_VALUE);
                if (newHistoryRect.right <= newHistoryRect.left) {
                    newHistoryRect.left = result.targetRect.left;
                    newHistoryRect.right = result.targetRect.right;
                }
                newHistoryRect.width = newHistoryRect.right - newHistoryRect.left;
                newHistoryRect.height = Number.MAX_VALUE;
                newHistoryRect.top = Number.MIN_VALUE;
                newHistoryRect.bottom = Number.MAX_VALUE;
            }
            _historyRect = newHistoryRect;
        }
    }
    function _findNextFocusElementInternal(direction, options) {
        options = options || {};
        options.focusRoot = options.focusRoot || _focusRoot || document.body;
        options.historyRect = options.historyRect || _defaultRect();
        var maxDistance = Math.max(window.screen.availHeight, window.screen.availWidth);
        var refObj = getReferenceObject(options.referenceElement, options.referenceRect);
        // Handle override
        var refElement = refObj.element;
        if (refElement) {
            var overrideSelector = refElement.getAttribute("data-tv-focus-" + direction);
            if (overrideSelector) {
                if (overrideSelector) {
                    var target;
                    var element = refObj.element;
                    while (!target && element) {
                        target = element.querySelector(overrideSelector);
                        element = element.parentElement;
                    }
                    if (target) {
                        if (target === document.activeElement) {
                            return null;
                        }
                        return { target: target, targetRect: _toIRect(target.getBoundingClientRect()), referenceRect: refObj.rect, usedOverride: true };
                    }
                }
            }
        }
        // Calculate scores for each element in the root
        var bestPotential = {
            element: null,
            rect: null,
            score: 0
        };
        var allElements = options.focusRoot.querySelectorAll("*");
        for (var i = 0, length = allElements.length; i < length; i++) {
            var potentialElement = allElements[i];
            if (refObj.element === potentialElement || !_isFocusable(potentialElement)) {
                continue;
            }
            var potentialRect = _toIRect(potentialElement.getBoundingClientRect());
            // Skip elements that have either a width of zero or a height of zero
            if (potentialRect.width === 0 || potentialRect.height === 0) {
                continue;
            }
            var score = calculateScore(direction, maxDistance, options.historyRect, refObj.rect, potentialRect);
            if (score > bestPotential.score) {
                bestPotential.element = potentialElement;
                bestPotential.rect = potentialRect;
                bestPotential.score = score;
            }
        }
        return bestPotential.element ? { target: bestPotential.element, targetRect: bestPotential.rect, referenceRect: refObj.rect, usedOverride: false } : null;
        // Nested Helpers
        function calculatePercentInShadow(minReferenceCoord, maxReferenceCoord, minPotentialCoord, maxPotentialCoord) {
            /// Calculates the percentage of the potential element that is in the shadow of the reference element.
            if ((minReferenceCoord >= maxPotentialCoord) || (maxReferenceCoord <= minPotentialCoord)) {
                // Potential is not in the reference's shadow.
                return 0;
            }
            var pixelOverlap = Math.min(maxReferenceCoord, maxPotentialCoord) - Math.max(minReferenceCoord, minPotentialCoord);
            var shortEdge = Math.min(maxPotentialCoord - minPotentialCoord, maxReferenceCoord - minReferenceCoord);
            return shortEdge === 0 ? 0 : (pixelOverlap / shortEdge);
        }
        function calculateScore(direction, maxDistance, historyRect, referenceRect, potentialRect) {
            var score = 0;
            var percentInShadow;
            var primaryAxisDistance;
            var secondaryAxisDistance = 0;
            var percentInHistoryShadow = 0;
            switch (direction) {
                case DirectionNames.left:
                    // Make sure we don't evaluate any potential elements to the right of the reference element
                    if (potentialRect.left >= referenceRect.left) {
                        break;
                    }
                    percentInShadow = calculatePercentInShadow(referenceRect.top, referenceRect.bottom, potentialRect.top, potentialRect.bottom);
                    primaryAxisDistance = referenceRect.left - potentialRect.right;
                    if (percentInShadow > 0) {
                        percentInHistoryShadow = calculatePercentInShadow(historyRect.top, historyRect.bottom, potentialRect.top, potentialRect.bottom);
                    }
                    else {
                        // If the potential element is not in the shadow, then we calculate secondary axis distance
                        secondaryAxisDistance = (referenceRect.bottom <= potentialRect.top) ? (potentialRect.top - referenceRect.bottom) : referenceRect.top - potentialRect.bottom;
                    }
                    break;
                case DirectionNames.right:
                    // Make sure we don't evaluate any potential elements to the left of the reference element
                    if (potentialRect.right <= referenceRect.right) {
                        break;
                    }
                    percentInShadow = calculatePercentInShadow(referenceRect.top, referenceRect.bottom, potentialRect.top, potentialRect.bottom);
                    primaryAxisDistance = potentialRect.left - referenceRect.right;
                    if (percentInShadow > 0) {
                        percentInHistoryShadow = calculatePercentInShadow(historyRect.top, historyRect.bottom, potentialRect.top, potentialRect.bottom);
                    }
                    else {
                        // If the potential element is not in the shadow, then we calculate secondary axis distance
                        secondaryAxisDistance = (referenceRect.bottom <= potentialRect.top) ? (potentialRect.top - referenceRect.bottom) : referenceRect.top - potentialRect.bottom;
                    }
                    break;
                case DirectionNames.up:
                    // Make sure we don't evaluate any potential elements below the reference element
                    if (potentialRect.top >= referenceRect.top) {
                        break;
                    }
                    percentInShadow = calculatePercentInShadow(referenceRect.left, referenceRect.right, potentialRect.left, potentialRect.right);
                    primaryAxisDistance = referenceRect.top - potentialRect.bottom;
                    if (percentInShadow > 0) {
                        percentInHistoryShadow = calculatePercentInShadow(historyRect.left, historyRect.right, potentialRect.left, potentialRect.right);
                    }
                    else {
                        // If the potential element is not in the shadow, then we calculate secondary axis distance
                        secondaryAxisDistance = (referenceRect.right <= potentialRect.left) ? (potentialRect.left - referenceRect.right) : referenceRect.left - potentialRect.right;
                    }
                    break;
                case DirectionNames.down:
                    // Make sure we don't evaluate any potential elements above the reference element
                    if (potentialRect.bottom <= referenceRect.bottom) {
                        break;
                    }
                    percentInShadow = calculatePercentInShadow(referenceRect.left, referenceRect.right, potentialRect.left, potentialRect.right);
                    primaryAxisDistance = potentialRect.top - referenceRect.bottom;
                    if (percentInShadow > 0) {
                        percentInHistoryShadow = calculatePercentInShadow(historyRect.left, historyRect.right, potentialRect.left, potentialRect.right);
                    }
                    else {
                        // If the potential element is not in the shadow, then we calculate secondary axis distance
                        secondaryAxisDistance = (referenceRect.right <= potentialRect.left) ? (potentialRect.left - referenceRect.right) : referenceRect.left - potentialRect.right;
                    }
                    break;
            }
            if (primaryAxisDistance >= 0) {
                // The score needs to be a positive number so we make these distances positive numbers
                primaryAxisDistance = maxDistance - primaryAxisDistance;
                secondaryAxisDistance = maxDistance - secondaryAxisDistance;
                if (primaryAxisDistance >= 0 && secondaryAxisDistance >= 0) {
                    // Potential elements in the shadow get a multiplier to their final score
                    primaryAxisDistance += primaryAxisDistance * percentInShadow;
                    score = primaryAxisDistance * ScoringConstants.primaryAxisDistanceWeight + secondaryAxisDistance * ScoringConstants.secondaryAxisDistanceWeight + percentInHistoryShadow * ScoringConstants.percentInHistoryShadowWeight;
                }
            }
            return score;
        }
        function getReferenceObject(referenceElement, referenceRect) {
            var refElement;
            var refRect;
            if ((!referenceElement && !referenceRect) || (referenceElement && !referenceElement.parentNode)) {
                // Note: We need to check to make sure 'parentNode' is not null otherwise there is a case
                // where _lastTarget is defined, but calling getBoundingClientRect will throw a native exception.
                // This case happens if the innerHTML of the parent of the _lastTarget is set to "".
                // If no valid reference is supplied, we'll use document.activeElement unless it's the body
                if (document.activeElement !== document.body) {
                    referenceElement = document.activeElement;
                }
            }
            if (referenceElement) {
                refElement = referenceElement;
                refRect = _toIRect(refElement.getBoundingClientRect());
            }
            else if (referenceRect) {
                refRect = _toIRect(referenceRect);
            }
            else {
                refRect = _defaultRect();
            }
            return {
                element: refElement,
                rect: refRect
            };
        }
    }
    function _defaultRect() {
        // We set the top, left, bottom and right properties of the referenceBoundingRectangle to '-1'
        // (as opposed to '0') because we want to make sure that even elements that are up to the edge
        // of the screen can receive focus.
        return {
            top: -1,
            bottom: -1,
            right: -1,
            left: -1,
            height: 0,
            width: 0
        };
    }
    function _toIRect(rect) {
        return {
            top: Math.floor(rect.top),
            bottom: Math.floor(rect.top + rect.height),
            right: Math.floor(rect.left + rect.width),
            left: Math.floor(rect.left),
            height: Math.floor(rect.height),
            width: Math.floor(rect.width),
        };
    }
    function _trySetFocus(element, keyCode) {
        // We raise an event on the focusRoot before focus changes to give listeners
        // a chance to prevent the next focus target from receiving focus if they want.
        var canceled = eventSrc.dispatchEvent(EventNames.focusChanging, { nextFocusElement: element, keyCode: keyCode });
        if (!canceled) {
            element.focus();
        }
        return document.activeElement === element;
    }
    function _isFocusable(element) {
        var elementTagName = element.tagName;
        var tabIndex = parseInt(element.getAttribute("tabIndex"));
        if (FocusableTagNames.indexOf(elementTagName) === -1 && isNaN(tabIndex)) {
            // Loop through the selectors
            var matchesSelector = false;
            for (var i = 0, len = FocusableSelectors.length; i < len; i++) {
                if (_matchesSelector(element, FocusableSelectors[i])) {
                    if (isNaN(tabIndex)) {
                        element.setAttribute("tabIndex", 0);
                    }
                    matchesSelector = true;
                    break;
                }
            }
            // If the current potential element is not one of the tags we consider to be focusable, then exit
            if (!matchesSelector) {
                return false;
            }
        }
        if (elementTagName === "IFRAME" && !IFrameHelper.isXYFocusEnabled(element)) {
            // Skip IFRAMEs without compatible XYFocus implementation
            return false;
        }
        if (elementTagName === "DIV" && element["winControl"] && element["winControl"].disabled) {
            // Skip disabled WinJS controls
            return false;
        }

        var style = window.getComputedStyle(element);
        if (style && tabIndex === -1 || style.display === "none" || style.visibility === "hidden" || element.disabled) {
            // Skip elements that are hidden
            // Note: We don't check for opacity === 0, because the browser cannot tell us this value accurately.
            return false;
        }
        return true;
    };
    function _matchesSelector(element, selectorString) {
        var matchesSelector = element.matches
                || element.msMatchesSelector
                || element.mozMatchesSelector
                || element.webkitMatchesSelector;
        return matchesSelector.call(element, selectorString);
    };
    function _handleKeyDownEvent(e) {
        if (e.defaultPrevented) {
            return;
        }
        var direction = "";
        var keyCode = e.keyCode;
        if (_keyCodeMap.up.indexOf(keyCode) !== -1) {
            direction = "up";
        }
        else if (_keyCodeMap.down.indexOf(keyCode) !== -1) {
            direction = "down";
        }
        else if (_keyCodeMap.left.indexOf(keyCode) !== -1) {
            direction = "left";
        }
        else if (_keyCodeMap.right.indexOf(keyCode) !== -1) {
            direction = "right";
        }
        if (direction && _enabled) {
            var shouldPreventDefault = _xyFocus(direction, keyCode);
            if (shouldPreventDefault) {
                e.preventDefault();
            }
        }
    };
    function _handleKeyUpEvent(e) {
        if (e.defaultPrevented) {
            return;
        }
        if (_keyCodeMap.accept.indexOf(e.keyCode) !== -1) {
            e.srcElement.click();
        }
    };

    var WebViewHelper;
    (function (WebViewHelper) {
        function refRectToNavigateFocusRect(refRect) {
            return {
                originLeft: refRect.left,
                originTop: refRect.top,
                originWidth: refRect.width,
                originHeight: refRect.height
            };
        }
        WebViewHelper.refRectToNavigateFocusRect = refRectToNavigateFocusRect;

        function navigateFocusRectToRefRect(navigateFocusRect) {
            return _toIRect({
                left: navigateFocusRect.originLeft,
                top: navigateFocusRect.originTop,
                width: navigateFocusRect.originWidth,
                height: navigateFocusRect.originHeight
            });
        }
        WebViewHelper.navigateFocusRectToRefRect = navigateFocusRectToRefRect;
    })(WebViewHelper || (WebViewHelper = {}));

    var IFrameHelper;
    (function (IFrameHelper) {
        // XYFocus caches registered iframes and iterates over the cache for its focus navigation implementation.
        // However, since there is no reliable way for an iframe to unregister with its parent as it can be
        // spontaneously taken out of the DOM, the cache can go stale. This helper module makes sure that on
        // every query to the iframe cache, stale iframes are removed.
        // Furthermore, merely accessing an iframe that has been garbage collected by the platform will cause an
        // exception so each iteration during a query must be in a try/catch block.
        var iframes = [];
        function count() {
            // Iterating over it causes stale iframes to be cleared from the cache.
            _safeForEach(function () { return false; });
            return iframes.length;
        }
        IFrameHelper.count = count;
        function getIFrameFromWindow(win) {
            var iframes = document.querySelectorAll("IFRAME");
            var found = Array.prototype.filter.call(iframes, function (x) { return x.contentWindow === win; });
            return found.length ? found[0] : null;
        }
        IFrameHelper.getIFrameFromWindow = getIFrameFromWindow;
        function isXYFocusEnabled(iframe) {
            var found = false;
            _safeForEach(function (ifr) {
                if (ifr === iframe) {
                    found = true;
                }
            });
            return found;
        }
        IFrameHelper.isXYFocusEnabled = isXYFocusEnabled;
        function registerIFrame(iframe) {
            iframes.push(iframe);
        };
        IFrameHelper.registerIFrame = registerIFrame;
        function unregisterIFrame(iframe) {
            var index = -1;
            _safeForEach(function (ifr, i) {
                if (ifr === iframe) {
                    index = i;
                }
            });
            if (index !== -1) {
                iframes.splice(index, 1);
            }
        };
        IFrameHelper.unregisterIFrame = unregisterIFrame;
        function _safeForEach(callback) {
            for (var i = iframes.length - 1; i >= 0; i--) {
                try {
                    var iframe = iframes[i];
                    if (!iframe.contentWindow) {
                        iframes.splice(i, 1);
                    }
                    else {
                        callback(iframe, i);
                    }
                }
                catch (e) {
                    // Iframe has been GC'd
                    iframes.splice(i, 1);
                }
            }
        };
    })(IFrameHelper || (IFrameHelper = {}));

    // Default mappings
    _keyCodeMap.left.push(
        37, // LeftArrow
        214, // GamepadLeftThumbstickLeft
        205, // GamepadDPadLeft
        140); // NavigationLeft
    _keyCodeMap.right.push(
        39, // RightArrow
        213, // GamepadLeftThumbstickRight
        206, // GamepadDPadRight
        141); // NavigationRight
    _keyCodeMap.up.push(
        38, // UpArrow
        211, // GamepadLeftThumbstickUp
        203, // GamepadDPadUp
        138); // NavigationUp
    _keyCodeMap.down.push(
        40, // UpArrow
        212, // GamepadLeftThumbstickDown
        204, // GamepadDPadDown
        139); // NavigationDown
    _keyCodeMap.accept.push(
        142, // NavigationAccept
        195); // GamepadA
    window.addEventListener("message", function (e) {
        // Note: e.source is the Window object of an iframe which could be hosting content
        // from a different domain. No properties on e.source should be accessed or we may
        // run into a cross-domain access violation exception.
        var sourceWindow = null;
        try {
            // Since messages are async, by the time we get this message, the iframe could've
            // been removed from the DOM and e.source is null or throws an exception on access.
            sourceWindow = e.source;
            if (!sourceWindow) {
                return;
            }
        }
        catch (e) {
            return;
        }
        if (!e.data || !e.data[CrossDomainMessageConstants.messageDataProperty]) {
            return;
        }
        var data = e.data[CrossDomainMessageConstants.messageDataProperty];
        switch (data.type) {
            case CrossDomainMessageConstants.register:
                var iframe = IFrameHelper.getIFrameFromWindow(sourceWindow);
                iframe && IFrameHelper.registerIFrame(iframe);
                break;
            case CrossDomainMessageConstants.unregister:
                var iframe = IFrameHelper.getIFrameFromWindow(sourceWindow);
                iframe && IFrameHelper.unregisterIFrame(iframe);
                break;
            case CrossDomainMessageConstants.dFocusEnter:
                // The coordinates stored in data.refRect are already in this frame's coordinate system.
                // First try to focus anything within this iframe without leaving the current frame.
                var focused = _xyFocus(data.direction, -1, data.referenceRect, true);
                if (!focused) {
                    // No focusable element was found, we'll focus document.body if it is focusable.
                    if (_isFocusable(document.body)) {
                        document.body.focus();
                    }
                    else {
                        // Nothing within this iframe is focusable, we call _xyFocus again without a refRect
                        // and allow the request to propagate to the parent.
                        _xyFocus(data.direction, -1);
                    }
                }
                break;
            case CrossDomainMessageConstants.dFocusExit:
                var iframe = IFrameHelper.getIFrameFromWindow(sourceWindow);
                if (document.activeElement !== iframe) {
                    break;
                }
                // The coordinates stored in data.refRect are in the IFRAME's coordinate system,
                // so we must first transform them into this frame's coordinate system.
                var refRect = data.referenceRect;
                var iframeRect = iframe.getBoundingClientRect();
                refRect.left += iframeRect.left;
                refRect.top += iframeRect.top;
                if (typeof refRect.right === "number") {
                    refRect.right += iframeRect.left;
                }
                if (typeof refRect.bottom === "number") {
                    refRect.bottom += iframeRect.top;
                }
                _xyFocus(data.direction, -1, refRect);
                break;
        }
    });

    // Receiving departingFocus event in the app as a result of any child webview calling
    // window.departFocus from within the webview. Indicates focus transitioning into the app
    // from the webview. The navigatingfocus event handles transforming the
    // coordinate space so we just pass the values along.
    document.addEventListener("departingfocus", function(eventArg) {
        var focusChanged = _xyFocus(
            eventArg.navigationReason,
            -1,
            WebViewHelper.navigateFocusRectToRefRect(eventArg));

        if (focusChanged && eventArg && (typeof eventArg.focus === "function")) {
            eventArg.focus();
        }
    });

    // Receiving a navigatingfocus event in the webview as a result of the host app calling
    // webview.navigateFocus on our containing webview element indicating focus transitioning
    // into the webview from the app. The navigatingfocus event handles transforming the
    // coordinate space so we just pass the values along.
    window.addEventListener("navigatingfocus", function(eventArg) {
        var focusChanged = _xyFocus(
            eventArg.navigationReason,
            -1,
            WebViewHelper.navigateFocusRectToRefRect(eventArg));

        if (focusChanged && eventArg && (typeof eventArg.focus === "function")) {
            eventArg.focus();
        }
    });

    var _initRun = false;
    var _init = function () {
        if (_initRun) {
            return;
        }
        _initRun = true;
        // Subscribe on bubble phase to allow developers to override XYFocus behaviors for directional keys.
        document.addEventListener("keydown", _handleKeyDownEvent);
        document.addEventListener("keyup", _handleKeyUpEvent);
        // If we are running within an iframe, we send a registration message to the parent window
        if (window.top !== window.window) {
            var message = {};
            message[CrossDomainMessageConstants.messageDataProperty] = {
                type: CrossDomainMessageConstants.register,
                version: 1.0
            };
            window.parent.postMessage(message, "*");
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        _init();
    });

    var EventMixinEvent = (function () {
        function EventMixinEvent(type, detail, target) {
            this.detail = detail;
            this.target = target;
            this.timeStamp = Date.now();
            this.type = type;
            this.bubbles = { value: false, writable: false };
            this.cancelable = { value: false, writable: false };
            this.trusted = { value: false, writable: false };
            this.eventPhase = { value: 0, writable: false };
            this.supportedForProcessing = true;
        };
        Object.defineProperty(EventMixinEvent.prototype, "currentTarget", {
            get: function () { return this.target; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(EventMixinEvent.prototype, "defaultPrevented", {
            get: function () { return this._preventDefaultCalled; },
            enumerable: true,
            configurable: true
        });
        EventMixinEvent.prototype.preventDefault = function () {
            this._preventDefaultCalled = true;
        };
        EventMixinEvent.prototype.stopImmediatePropagation = function () {
            this._stopImmediatePropagationCalled = true;
        };
        EventMixinEvent.prototype.stopPropagation = function () {
        };
        return EventMixinEvent;
    })();

    var eventMixin = {
        _listeners: null,

        addEventListener: function (type, listener, useCapture) {
            /// <signature helpKeyword="WinJS.Utilities.eventMixin.addEventListener">
            /// <summary locid="WinJS.Utilities.eventMixin.addEventListener">
            /// Adds an event listener to the control.
            /// </summary>
            /// <param name="type" locid="WinJS.Utilities.eventMixin.addEventListener_p:type">
            /// The type (name) of the event.
            /// </param>
            /// <param name="listener" locid="WinJS.Utilities.eventMixin.addEventListener_p:listener">
            /// The listener to invoke when the event is raised.
            /// </param>
            /// <param name="useCapture" locid="WinJS.Utilities.eventMixin.addEventListener_p:useCapture">
            /// if true initiates capture, otherwise false.
            /// </param>
            /// </signature>
            useCapture = useCapture || false;
            this._listeners = this._listeners || {};
            var eventListeners = (this._listeners[type] = this._listeners[type] || []);
            for (var i = 0, len = eventListeners.length; i < len; i++) {
                var l = eventListeners[i];
                if (l.useCapture === useCapture && l.listener === listener) {
                    return;
                }
            }
            eventListeners.push({ listener: listener, useCapture: useCapture });
        },
        dispatchEvent: function (type, details) {
            /// <signature helpKeyword="WinJS.Utilities.eventMixin.dispatchEvent">
            /// <summary locid="WinJS.Utilities.eventMixin.dispatchEvent">
            /// Raises an event of the specified type and with the specified additional properties.
            /// </summary>
            /// <param name="type" locid="WinJS.Utilities.eventMixin.dispatchEvent_p:type">
            /// The type (name) of the event.
            /// </param>
            /// <param name="details" locid="WinJS.Utilities.eventMixin.dispatchEvent_p:details">
            /// The set of additional properties to be attached to the event object when the event is raised.
            /// </param>
            /// <returns type="Boolean" locid="WinJS.Utilities.eventMixin.dispatchEvent_returnValue">
            /// true if preventDefault was called on the event.
            /// </returns>
            /// </signature>
            var listeners = this._listeners && this._listeners[type];
            if (listeners) {
                var eventValue = new EventMixinEvent(type, details, this);
                // Need to copy the array to protect against people unregistering while we are dispatching
                listeners = listeners.slice(0, listeners.length);
                for (var i = 0, len = listeners.length; i < len && !eventValue._stopImmediatePropagationCalled; i++) {
                    listeners[i].listener(eventValue);
                }
                return eventValue.defaultPrevented || false;
            }
            return false;
        },
        removeEventListener: function (type, listener, useCapture) {
            /// <signature helpKeyword="WinJS.Utilities.eventMixin.removeEventListener">
            /// <summary locid="WinJS.Utilities.eventMixin.removeEventListener">
            /// Removes an event listener from the control.
            /// </summary>
            /// <param name="type" locid="WinJS.Utilities.eventMixin.removeEventListener_p:type">
            /// The type (name) of the event.
            /// </param>
            /// <param name="listener" locid="WinJS.Utilities.eventMixin.removeEventListener_p:listener">
            /// The listener to remove.
            /// </param>
            /// <param name="useCapture" locid="WinJS.Utilities.eventMixin.removeEventListener_p:useCapture">
            /// Specifies whether to initiate capture.
            /// </param>
            /// </signature>
            useCapture = useCapture || false;
            var listeners = this._listeners && this._listeners[type];
            if (listeners) {
                for (var i = 0, len = listeners.length; i < len; i++) {
                    var l = listeners[i];
                    if (l.listener === listener && l.useCapture === useCapture) {
                        listeners.splice(i, 1);
                        if (listeners.length === 0) {
                            delete this._listeners[type];
                        }
                        // Only want to remove one element for each call to removeEventListener
                        break;
                    }
                }
            }
        }
    };
    // Publish to WinJS namespace
    var toPublish = {
        init: _init,
        findNextFocusElement: _findNextFocusElement,
        keyCodeMap: _keyCodeMap,
        focusableSelectors: FocusableSelectors,
        moveFocus: _moveFocus,
        onfocuschanged: _createEventProperty(EventNames.focusChanged),
        onfocuschanging: _createEventProperty(EventNames.focusChanging),
        _xyFocus: _xyFocus,
        _handleKeyDownEvent: _handleKeyDownEvent,
        _handleKeyUpEvent: _handleKeyUpEvent,
        _iframeHelper: IFrameHelper
    };
    toPublish = _mergeAll([toPublish, eventMixin]);
    toPublish["_listeners"] = {};
    var eventSrc = toPublish;
    window.TVJS = window.TVJS || {};
    TVJS.DirectionalNavigation = toPublish;
    Object.defineProperty(window.TVJS.DirectionalNavigation, "enabled", {
        get: function () {
            return _enabled;
        },
        set: function (value) {
            _enabled = value;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(window.TVJS.DirectionalNavigation, "focusRoot", {
        get: function () {
            return _focusRoot;
        },
        set: function (value) {
            _focusRoot = value;
        },
        enumerable: true,
        configurable: true
    });

    // The gamepadInputEmulation is a string property that exists in JavaScript UWAs and in WebViews in UWAs.
    // It won't exist in Win8.1 style apps or browsers.
    if (window.navigator && typeof window.navigator.gamepadInputEmulation === "string") {
        // We want the gamepad to provide gamepad VK keyboard events rather than moving a
        // mouse like cursor. Set to "keyboard", the gamepad will provide such keyboard events
        // and provide input to the DOM navigator.getGamepads API.
        window.navigator.gamepadInputEmulation = "keyboard";
    }
})();
