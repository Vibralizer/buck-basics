# Menus (UI Toolkit)

The UI Toolkit counterpart to the uGUI Menus module. It provides the same
mental model (a controller managing a stack of screens, variables bound to
controls, pointer-vs-navigation styling) on top of `UnityEngine.UIElements`
and the Unity 6 runtime data binding system. Everything in this folder is
compiled only on Unity 6000.0 or newer; the uGUI Menus module is unaffected.

## Classes

| Class | Purpose | uGUI counterpart |
|---|---|---|
| `MenuStack` | Plain-C# stack of screens: push, pop, sibling swap, close all, cancel routing, stack events including empty-changed | `MenuController` |
| `IMenuScreen` / `MenuScreenBase` | A screen the stack shows/hides, with initial-focus selection and an optional cancel-consume hook | `MenuScreen` |
| `BoolVariableSource`, `FloatVariableSource`, `IntVariableSource` | Adapters exposing BUCK Variables to UI Toolkit data binding (`[CreateProperty]` Value + `INotifyBindablePropertyChanged`). A UI-initiated change writes the Variable and optionally raises its GameEvent, exactly like `UIToggleHelper`/`UISliderHelper` | `VariableBinding` + helpers |
| `UIInputModeClassDriver` | Toggles `mode-pointer` / `mode-navigation` classes on the panel root from a BoolVariable | `UiInputMode` + `SelectableColorsProfile` |
| `MenuCancelRouter` | Routes the UI cancel action to an ordered list of stacks, topmost first | per-controller cancel handling |

## Typical wiring

```csharp
var stack = new MenuStack("settings", layerRoot);
stack.OnStackEmptyChanged += empty => hud.SetMenuOpen(!empty);

var screen = new SettingsScreen(settingsRoot); // MenuScreenBase subclass
stack.Push(screen);

// Variable binding: a Toggle bound TwoWay to a BoolVariable.
var source = new BoolVariableSource(myBoolVariable, raiseGameEventOnChange: true);
source.Attach();
toggle.dataSource = source;
toggle.SetBinding("value", new DataBinding
{
    dataSourcePath = new PropertyPath(nameof(BoolVariableSource.Value)),
    bindingMode = BindingMode.TwoWay
});
// In OnDisable: toggle.ClearBinding("value"); source.Detach();
```

## USS class-name contract

The module owns behavior; styling is the consuming project's job via these
class names:

- `mode-pointer` / `mode-navigation` on the panel root: gate every
  state-styling rule on one of these. Pointer mode styles `:hover` and
  `:active`; navigation mode styles `:focus` (and should make focus loud,
  for example a full-row background highlight, since it is the only cue a
  gamepad or keyboard user has).
- `menu-item` on each focusable row: `MenuScreenBase.GetInitialFocus()`
  focuses the first focusable `menu-item` when a screen is shown.
- `menu-item__label`, `menu-item__indicator`: conventional children of a row
  for the label and an optional focus indicator.
- `menu-screen` on each screen root (conventional; not required by code).

## Show/hide semantics

`MenuScreenBase.Show`/`Hide` are immediate display toggles, mirroring the
uGUI `MenuScreen` CanvasGroup flip. Subclasses that animate should override
`Show`/`Hide` and invoke the base when the transition lands; the stack itself
is intentionally synchronous.
