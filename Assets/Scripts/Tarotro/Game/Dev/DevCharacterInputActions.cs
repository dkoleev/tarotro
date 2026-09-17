using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Tarotro.Game.Dev {
    public class DevCharacterInputActions : IInputActionCollection2, IDisposable {
        public InputActionAsset asset { get; }

        public DevCharacterInputActions() {
            asset = InputActionAsset.FromJson(@"{
                ""name"": ""DevCharacterInputActions"",
                ""maps"": [
                    {
                        ""name"": ""Camera"",
                        ""id"": ""a1b2c3d4-0001-0001-0001-000000000001"",
                        ""actions"": [
                            {
                                ""name"": ""Move"",
                                ""type"": ""Value"",
                                ""id"": ""a1b2c3d4-0002-0002-0002-000000000001"",
                                ""expectedControlType"": ""Vector2"",
                                ""processors"": """",
                                ""interactions"": """",
                                ""initialStateCheck"": true
                            },
                            {
                                ""name"": ""Zoom"",
                                ""type"": ""Value"",
                                ""id"": ""a1b2c3d4-0002-0002-0002-000000000002"",
                                ""expectedControlType"": ""Axis"",
                                ""processors"": """",
                                ""interactions"": """",
                                ""initialStateCheck"": true
                            }
                        ],
                        ""bindings"": [
                            {
                                ""name"": ""WASD"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000001"",
                                ""path"": ""2DVector"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": true,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": ""up"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000002"",
                                ""path"": ""<Keyboard>/w"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""down"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000003"",
                                ""path"": ""<Keyboard>/s"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""left"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000004"",
                                ""path"": ""<Keyboard>/a"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""right"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000005"",
                                ""path"": ""<Keyboard>/d"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""Arrows"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000006"",
                                ""path"": ""2DVector"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": true,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": ""up"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000007"",
                                ""path"": ""<Keyboard>/upArrow"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""down"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000008"",
                                ""path"": ""<Keyboard>/downArrow"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""left"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000009"",
                                ""path"": ""<Keyboard>/leftArrow"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": ""right"",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000010"",
                                ""path"": ""<Keyboard>/rightArrow"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": """",
                                ""action"": ""Move"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": true
                            },
                            {
                                ""name"": """",
                                ""id"": ""a1b2c3d4-0003-0003-0003-000000000011"",
                                ""path"": ""<Mouse>/scroll/y"",
                                ""interactions"": """",
                                ""processors"": ""NormalizeVector2"",
                                ""groups"": """",
                                ""action"": ""Zoom"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            }
                        ]
                    }
                ],
                ""controlSchemes"": []
            }");
            _camera = new CameraActions(this);
        }

        public void Dispose() {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action) => asset.Contains(action);
        public IEnumerator<InputAction> GetEnumerator() => asset.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public void Enable() => asset.Enable();
        public void Disable() => asset.Disable();

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false) =>
            asset.FindAction(actionNameOrId, throwIfNotFound);

        public int FindBinding(InputBinding bindingMask, out InputAction action) =>
            asset.FindBinding(bindingMask, out action);

        public IEnumerable<InputBinding> bindings { get; }

        private readonly CameraActions _camera;
        public CameraActions Camera => _camera;

        public struct CameraActions {
            private readonly DevCharacterInputActions _wrapper;

            public CameraActions(DevCharacterInputActions wrapper) { _wrapper = wrapper; }

            public InputAction Move => _wrapper.asset.FindAction("Camera/Move", true);
            public InputAction Zoom => _wrapper.asset.FindAction("Camera/Zoom", true);
        }
    }
}
