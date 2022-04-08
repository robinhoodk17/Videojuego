using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class KeyBindings : MonoBehaviour
{
    public PlayerInput playerInput = null;
    [Header("Up")]
    public InputActionReference MoveUp = null;
    public TMP_Text UpButtonText = null;
    [Header("Down")]
    public InputActionReference MoveDown = null;
    public TMP_Text DownButtonText = null;
    [Header("Left")]
    public InputActionReference MoveLeft = null;
    public TMP_Text LeftButtonText = null;
    [Header("Right")]
    public InputActionReference MoveRight = null;
    public TMP_Text RightButtonText = null;
    [Header("ZoomIn")]
    public InputActionReference ZoomIn = null;
    public TMP_Text InButtonText = null;
    [Header("ZoomOut")]
    public InputActionReference ZoomOut = null;
    public TMP_Text OutButtonText = null;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    //public PlayerInputActions inputmap;
    private void Awake()
    {
        turnOn();
        string rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
        if(string.IsNullOrEmpty(rebinds)){return;}
        playerInput.actions.LoadBindingOverridesFromJson(rebinds);
        turnOff();
    }
    public void turnOn()
    {
        gameObject.SetActive(true);
        string rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
        if(string.IsNullOrEmpty(rebinds))
        {
            return;
        }
        playerInput.actions.LoadBindingOverridesFromJson(rebinds);

        UpButtonText.text = InputControlPath.ToHumanReadableString(
            MoveUp.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        DownButtonText.text = InputControlPath.ToHumanReadableString(
            MoveDown.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        RightButtonText.text = InputControlPath.ToHumanReadableString(
            MoveRight.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        LeftButtonText.text = InputControlPath.ToHumanReadableString(
            MoveLeft.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        InButtonText.text = InputControlPath.ToHumanReadableString(
            ZoomIn.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        OutButtonText.text = InputControlPath.ToHumanReadableString(
            ZoomOut.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    public void StartRebindingUp()
    {
        UpButtonText.gameObject.SetActive(false);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("Pause");
        rebindingOperation = MoveUp.action.PerformInteractiveRebinding()
        .WithControlsExcluding("Mouse")
        .OnMatchWaitForAnother(0.1f)
        .OnComplete(operation =>UpRebindComplete())
        .Start();
    }
    public void UpRebindComplete()
    {
        rebindingOperation.Dispose();
        UpButtonText.gameObject.SetActive(true);
        UpButtonText.text = InputControlPath.ToHumanReadableString(
            MoveUp.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CameraMovement");
    }
    public void StartRebindingDown()
    {
        DownButtonText.gameObject.SetActive(false);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("Pause");
        rebindingOperation = MoveDown.action.PerformInteractiveRebinding()
        .WithControlsExcluding("Mouse")
        .OnMatchWaitForAnother(0.1f)
        .OnComplete(operation =>DownRebindComplete())
        .Start();
    }
    public void DownRebindComplete()
    {
        rebindingOperation.Dispose();
        DownButtonText.gameObject.SetActive(true);
        DownButtonText.text = InputControlPath.ToHumanReadableString(
            MoveDown.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CameraMovement");
    }
    public void StartRebindingRight()
    {
        RightButtonText.gameObject.SetActive(false);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("Pause");
        rebindingOperation = MoveRight.action.PerformInteractiveRebinding()
        .WithControlsExcluding("Mouse")
        .OnMatchWaitForAnother(0.1f)
        .OnComplete(operation =>RightRebindComplete())
        .Start();
    }
    public void RightRebindComplete()
    {
        rebindingOperation.Dispose();
        RightButtonText.gameObject.SetActive(true);
        RightButtonText.text = InputControlPath.ToHumanReadableString(
            MoveRight.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CameraMovement");
    }
    public void StartRebindingLeft()
    {
        LeftButtonText.gameObject.SetActive(false);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("Pause");
        rebindingOperation = MoveLeft.action.PerformInteractiveRebinding()
        .WithControlsExcluding("Mouse")
        .OnMatchWaitForAnother(0.1f)
        .OnComplete(operation =>LeftRebindComplete())
        .Start();
    }
    public void LeftRebindComplete()
    {
        rebindingOperation.Dispose();
        LeftButtonText.gameObject.SetActive(true);
        LeftButtonText.text = InputControlPath.ToHumanReadableString(
            MoveLeft.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CameraMovement");
    }
    public void StartRebindingZoomIn()
    {
        InButtonText.gameObject.SetActive(false);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("Pause");
        rebindingOperation = ZoomIn.action.PerformInteractiveRebinding()
        .WithControlsExcluding("Mouse")
        .OnMatchWaitForAnother(0.1f)
        .OnComplete(operation =>ZoomInRebindComplete())
        .Start();
    }
    public void ZoomInRebindComplete()
    {
        rebindingOperation.Dispose();
        InButtonText.gameObject.SetActive(true);
        InButtonText.text = InputControlPath.ToHumanReadableString(
            ZoomIn.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CameraMovement");
    }
    public void StartRebindingZoomOut()
    {
        OutButtonText.gameObject.SetActive(false);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("Pause");
        rebindingOperation = ZoomOut.action.PerformInteractiveRebinding()
        .WithControlsExcluding("Mouse")
        .OnMatchWaitForAnother(0.1f)
        .OnComplete(operation =>ZoomOutRebindComplete())
        .Start();
    }
    public void ZoomOutRebindComplete()
    {
        rebindingOperation.Dispose();
        OutButtonText.gameObject.SetActive(true);
        OutButtonText.text = InputControlPath.ToHumanReadableString(
            ZoomOut.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CameraMovement");
    }

    public void SaveSettings()
    {
        PlayerInput playerInput = Camera.main.GetComponent<PlayerInput>();
        string rebinds = playerInput.actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
        gameObject.SetActive(false);
    }

    public void turnOff()
    {
        Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("Pause");
        string rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
        if(string.IsNullOrEmpty(rebinds))
        {
            playerInput.actions.RemoveAllBindingOverrides();
            Camera.main.GetComponent<PlayerInput>()?.SwitchCurrentActionMap("CameraMovement");
            return;
        }
        playerInput.actions.LoadBindingOverridesFromJson(rebinds);
        Camera.main.GetComponent<PlayerInput>()?.actions.LoadBindingOverridesFromJson(rebinds);
        UpButtonText.text = InputControlPath.ToHumanReadableString(
            MoveUp.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        DownButtonText.text = InputControlPath.ToHumanReadableString(
            MoveDown.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        RightButtonText.text = InputControlPath.ToHumanReadableString(
            MoveRight.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        LeftButtonText.text = InputControlPath.ToHumanReadableString(
            MoveLeft.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        InButtonText.text = InputControlPath.ToHumanReadableString(
            ZoomIn.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
        OutButtonText.text = InputControlPath.ToHumanReadableString(
            ZoomOut.action.bindings[0].effectivePath ,InputControlPath.HumanReadableStringOptions.OmitDevice);
            
        
        gameObject.SetActive(false);
    }

}
