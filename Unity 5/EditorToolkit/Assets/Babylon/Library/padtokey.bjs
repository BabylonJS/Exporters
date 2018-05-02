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
(function () {
    "use strict";

    var _GAMEPAD_A_BUTTON_INDEX = 0,
    _GAMEPAD_B_BUTTON_INDEX = 1,
    _GAMEPAD_DPAD_UP_BUTTON_INDEX = 12,
    _GAMEPAD_DPAD_DOWN_BUTTON_INDEX = 13,
    _GAMEPAD_DPAD_LEFT_BUTTON_INDEX = 14,
    _GAMEPAD_DPAD_RIGHT_BUTTON_INDEX = 15,
    _GAMEPAD_LEFT_SHOULDER_BUTTON_INDEX = 4,
    _GAMEPAD_RIGHT_SHOULDER_BUTTON_INDEX = 5,
    _GAMEPAD_A_KEY = "GamepadA",
    _GAMEPAD_B_KEY = "GamepadB",
    _GAMEPAD_DPAD_UP_KEY = "GamepadDPadUp",
    _GAMEPAD_DPAD_DOWN_KEY = "GamepadDPadDown",
    _GAMEPAD_DPAD_LEFT_KEY = "GamepadDPadLeft",
    _GAMEPAD_DPAD_RIGHT_KEY = "GamepadDPadRight",
    _GAMEPAD_LEFT_THUMBSTICK_UP_KEY = "GamepadLeftThumbStickUp",
    _GAMEPAD_LEFT_THUMBSTICK_DOWN_KEY = "GamepadLeftThumbStickDown",
    _GAMEPAD_LEFT_THUMBSTICK_LEFT_KEY = "GamepadLeftThumbStickLeft",
    _GAMEPAD_LEFT_THUMBSTICK_RIGHT_KEY = "GamepadLeftThumbStickRight",
    _GAMEPAD_LEFT_SHOULDER_KEY = "GamepadLeftShoulder",
    _GAMEPAD_RIGHT_SHOULDER_KEY = "GamepadRightShoulder",
    _GAMEPAD_A_KEYCODE = 195,
    _GAMEPAD_B_KEYCODE = 196,
    _GAMEPAD_DPAD_UP_KEYCODE = 203,
    _GAMEPAD_DPAD_DOWN_KEYCODE = 204,
    _GAMEPAD_DPAD_LEFT_KEYCODE = 205,
    _GAMEPAD_DPAD_RIGHT_KEYCODE = 206,
    _GAMEPAD_SHOULDER_LEFT_KEYCODE = 207,
    _GAMEPAD_SHOULDER_RIGHT_KEYCODE = 208,
    _GAMEPAD_LEFT_THUMBSTICK_UP_KEYCODE = 211,
    _GAMEPAD_LEFT_THUMBSTICK_DOWN_KEYCODE = 212,
    _GAMEPAD_LEFT_THUMBSTICK_LEFT_KEYCODE = 214,
    _GAMEPAD_LEFT_THUMBSTICK_RIGHT_KEYCODE = 213,
    _THUMB_STICK_THRESHOLD = 0.75;

    var _leftThumbstickUpPressed = false,
    _leftThumbstickDownPressed = false,
    _leftThumbstickLeftPressed = false,
    _leftThumbstickRightPressed = false,
    _dPadUpPressed = false,
    _dPadDownPressed = false,
    _dPadLeftPressed = false,
    _dPadRightPressed = false,
    _gamepadAPressed = false,
    _gamepadBPressed = false,
    _shoulderLeftPressed = false,
    _shoulderRightPressed = false;

    // The set of buttons on the gamepad we listen for.
    var ProcessedButtons = [
        _GAMEPAD_DPAD_UP_BUTTON_INDEX,
        _GAMEPAD_DPAD_DOWN_BUTTON_INDEX,
        _GAMEPAD_DPAD_LEFT_BUTTON_INDEX,
        _GAMEPAD_DPAD_RIGHT_BUTTON_INDEX,
        _GAMEPAD_A_BUTTON_INDEX,
        _GAMEPAD_B_BUTTON_INDEX,
        _GAMEPAD_LEFT_SHOULDER_BUTTON_INDEX,
        _GAMEPAD_RIGHT_SHOULDER_BUTTON_INDEX
    ];

    var _ButtonPressedState = {};
    Object.defineProperty(_ButtonPressedState, "gamepadA", {
        get: function () {
            return _gamepadAPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_gamepadAPressed, newPressedState, _GAMEPAD_A_KEY, _GAMEPAD_A_KEYCODE);
            _gamepadAPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "gamepadB", {
        get: function () {
            return _gamepadBPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_gamepadBPressed, newPressedState, _GAMEPAD_B_KEY, _GAMEPAD_B_KEYCODE);
            _gamepadBPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "leftThumbstickUp", {
        get: function () {
            return _leftThumbstickUpPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_leftThumbstickUpPressed, newPressedState, _GAMEPAD_LEFT_THUMBSTICK_UP_KEY, _GAMEPAD_LEFT_THUMBSTICK_UP_KEYCODE);
            _leftThumbstickUpPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "leftThumbstickDown", {
        get: function () {
            return _leftThumbstickDownPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_leftThumbstickDownPressed, newPressedState, _GAMEPAD_LEFT_THUMBSTICK_DOWN_KEY, _GAMEPAD_LEFT_THUMBSTICK_DOWN_KEYCODE);
            _leftThumbstickDownPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "leftThumbstickLeft", {
        get: function () {
            return _leftThumbstickLeftPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_leftThumbstickLeftPressed, newPressedState, _GAMEPAD_LEFT_THUMBSTICK_LEFT_KEY, _GAMEPAD_LEFT_THUMBSTICK_LEFT_KEYCODE);
            _leftThumbstickLeftPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "leftThumbstickRight", {
        get: function () {
            return _leftThumbstickRightPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_leftThumbstickRightPressed, newPressedState, _GAMEPAD_LEFT_THUMBSTICK_RIGHT_KEY, _GAMEPAD_LEFT_THUMBSTICK_RIGHT_KEYCODE);
            _leftThumbstickRightPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "dPadUp", {
        get: function () {
            return _dPadUpPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_dPadUpPressed, newPressedState, _GAMEPAD_DPAD_UP_KEY, _GAMEPAD_DPAD_UP_KEYCODE);
            _dPadUpPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "dPadDown", {
        get: function () {
            return _dPadDownPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_dPadDownPressed, newPressedState, _GAMEPAD_DPAD_DOWN_KEY, _GAMEPAD_DPAD_DOWN_KEYCODE);
            _dPadDownPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "dPadLeft", {
        get: function () {
            return _dPadLeftPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_dPadLeftPressed, newPressedState, _GAMEPAD_DPAD_LEFT_KEY, _GAMEPAD_DPAD_LEFT_KEYCODE);
            _dPadLeftPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "dPadRight", {
        get: function () {
            return _dPadRightPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_dPadRightPressed, newPressedState, _GAMEPAD_DPAD_RIGHT_KEY, _GAMEPAD_DPAD_RIGHT_KEYCODE);
            _dPadRightPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "shoulderLeft", {
        get: function () {
            return _shoulderLeftPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_shoulderLeftPressed, newPressedState, _GAMEPAD_LEFT_SHOULDER_KEY, _GAMEPAD_SHOULDER_LEFT_KEYCODE);
            _shoulderLeftPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(_ButtonPressedState, "shoulderRight", {
        get: function () {
            return _shoulderRightPressed;
        },
        set: function (newPressedState) {
            raiseKeyEvent(_shoulderRightPressed, newPressedState, _GAMEPAD_RIGHT_SHOULDER_KEY, _GAMEPAD_SHOULDER_RIGHT_KEYCODE);
            _shoulderRightPressed = newPressedState;
        },
        enumerable: true,
        configurable: true
    });

    function raiseEvent(name, key, keyCode) {
        var event = document.createEvent('Event');
        event.initEvent(name, true, true);
        event.key = key;
        event.keyCode = keyCode;
        document.body.dispatchEvent(event);
    };

    function raiseKeyEvent(oldPressedState, newPressedState, key, keyCode) {
        // No-op if oldPressedState === newPressedState
        if (newPressedState === true &&
            oldPressedState === false) { // button down
                console.log(key, keyCode)
            raiseEvent("keydown", key, keyCode);
        } else if (newPressedState === false &&
            oldPressedState === true) { // button up
            raiseEvent("keyup", key, keyCode);
        }
    };

    function runInputLoop() {
        // Get the latest gamepad state.
        var gamepads = navigator.getGamepads();
        for (var i = 0, len = gamepads.length; i < len; i++) {
            var gamepad = gamepads[i];
            if (gamepad) {
                // Iterate through the axes
                var axes = gamepad.axes;
                var leftStickX = axes[0];
                var leftStickY = axes[1];
                if (leftStickX > _THUMB_STICK_THRESHOLD) { // Right
                    _ButtonPressedState.leftThumbstickRight = true;
                } else if (leftStickX < -_THUMB_STICK_THRESHOLD) { // Left
                    _ButtonPressedState.leftThumbstickLeft = true;
                } else if (leftStickY < -_THUMB_STICK_THRESHOLD) { // Up
                    _ButtonPressedState.leftThumbstickUp = true;
                } else if (leftStickY > _THUMB_STICK_THRESHOLD) { // Down
                    _ButtonPressedState.leftThumbstickDown = true;
                } else {
                    _ButtonPressedState.leftThumbstickLeft = false;
                    _ButtonPressedState.leftThumbstickRight = false;
                    _ButtonPressedState.leftThumbstickUp = false;
                    _ButtonPressedState.leftThumbstickDown = false;
                }
                // Iterate through the buttons to see if Left thumbstick, DPad, A and B are pressed.
                var buttons = gamepad.buttons;
                for (var j = 0, len = buttons.length; j < len; j++) {
                    if (ProcessedButtons.indexOf(j) !== -1) {

                        if (buttons[j].pressed) {
                            switch (j) {
                                case _GAMEPAD_DPAD_UP_BUTTON_INDEX:
                                    _ButtonPressedState.dPadUp = true;
                                    break;
                                case _GAMEPAD_DPAD_DOWN_BUTTON_INDEX:
                                    _ButtonPressedState.dPadDown = true;
                                    break;
                                case _GAMEPAD_DPAD_LEFT_BUTTON_INDEX:
                                    _ButtonPressedState.dPadLeft = true;
                                    break;
                                case _GAMEPAD_DPAD_RIGHT_BUTTON_INDEX:
                                    _ButtonPressedState.dPadRight = true;
                                    break;
                                case _GAMEPAD_A_BUTTON_INDEX:
                                    _ButtonPressedState.gamepadA = true;
                                    break;
                                case _GAMEPAD_B_BUTTON_INDEX:
                                    _ButtonPressedState.gamepadB = true;
                                    break;
                                case _GAMEPAD_LEFT_SHOULDER_BUTTON_INDEX:
                                    _ButtonPressedState.shoulderLeft = true;
                                    break;
                                case _GAMEPAD_RIGHT_SHOULDER_BUTTON_INDEX:
                                    _ButtonPressedState.shoulderRight = true;
                                    break;
                                default:
                                    // No-op
                                    break;
                            };
                        } else {
                            switch (j) {
                                case _GAMEPAD_DPAD_UP_BUTTON_INDEX:
                                    if (_ButtonPressedState.dPadUp) {
                                        _ButtonPressedState.dPadUp = false;
                                    }
                                    break;
                                case _GAMEPAD_DPAD_DOWN_BUTTON_INDEX:
                                    if (_ButtonPressedState.dPadDown) {
                                        _ButtonPressedState.dPadDown = false;
                                    }
                                    break;
                                case _GAMEPAD_DPAD_LEFT_BUTTON_INDEX:
                                    if (_ButtonPressedState.dPadLeft) {
                                        _ButtonPressedState.dPadLeft = false;
                                    }
                                    break;
                                case _GAMEPAD_DPAD_RIGHT_BUTTON_INDEX:
                                    if (_ButtonPressedState.dPadRight) {
                                        _ButtonPressedState.dPadRight = false;
                                    }
                                    break;
                                case _GAMEPAD_A_BUTTON_INDEX:
                                    if (_ButtonPressedState.gamepadA) {
                                        _ButtonPressedState.gamepadA = false;
                                    }
                                    break;
                                case _GAMEPAD_B_BUTTON_INDEX:
                                    if (_ButtonPressedState.gamepadB) {
                                        _ButtonPressedState.gamepadB = false;
                                    }
                                    break;
                                case _GAMEPAD_LEFT_SHOULDER_BUTTON_INDEX:
                                    if (_ButtonPressedState.shoulderLeft) {
                                        _ButtonPressedState.shoulderLeft = false;
                                    }
                                    break;
                                case _GAMEPAD_RIGHT_SHOULDER_BUTTON_INDEX:
                                    if (_ButtonPressedState.shoulderRight) {
                                        _ButtonPressedState.shoulderRight = false;
                                    }
                                    break;
                                default:
                                    // No-op
                                    break;
                            };
                        }
                    }
                }
            }
        }
        // Schedule the next one
        requestAnimationFrame(runInputLoop);
    };

    runInputLoop();
})();
