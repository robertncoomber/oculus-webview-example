using UnityEngine;
using UnityEngine.Serialization;
using Vuplex.WebView;
using UnityEngine.XR;

class OculusWebViewDemo : MonoBehaviour
{
    public WebPluginType WebPluginType;
    public DragMode DragMode;
    public bool SetupPopupHandler;
    
    WebViewPrefab _webViewPrefab;
    Keyboard _keyboard;
    
    async void Start() {

        // Use a desktop User-Agent to request the desktop versions of websites.
        // https://developer.vuplex.com/webview/Web#SetUserAgent
        Web.SetUserAgent(false);

        // Create a 0.6 x 0.4 instance of the prefab.
        // https://developer.vuplex.com/webview/WebViewPrefab#Instantiate
        _webViewPrefab = WebViewPrefab.Instantiate(0.6f, 0.4f, new WebViewOptions()
        {
            preferredPlugins = new WebPluginType[] { WebPluginType }
        });
        _webViewPrefab.DragMode = DragMode;
        
        _webViewPrefab.transform.SetParent(transform, false);
        _webViewPrefab.transform.localPosition = new Vector3(0, 0.2f, 0.6f);
        _webViewPrefab.transform.localEulerAngles = new Vector3(0, 180, 0);

        // Add an on-screen keyboard under the webview.
        // https://developer.vuplex.com/webview/Keyboard
        _keyboard = Keyboard.Instantiate();
        _keyboard.transform.SetParent(_webViewPrefab.transform, false);
        _keyboard.transform.localPosition = new Vector3(0, -0.41f, 0);
        _keyboard.transform.localEulerAngles = Vector3.zero;

        // Wait for the prefab to initialize because its WebView property is null until then.
        // https://developer.vuplex.com/webview/WebViewPrefab#WaitUntilInitialized
        await _webViewPrefab.WaitUntilInitialized();

        if (SetupPopupHandler)
        {
            var webViewWithPopups = _webViewPrefab.WebView as IWithPopups;
            if (webViewWithPopups != null) {
                webViewWithPopups.SetPopupMode(PopupMode.LoadInNewWebView);

                webViewWithPopups.PopupRequested += async (sender, eventArgs) => {
                    Debug.Log("Popup opened with URL: " + eventArgs.Url);
                    
                    var popupPrefab = WebViewPrefab.Instantiate(eventArgs.WebView);
                    popupPrefab.transform.parent = transform;
                    popupPrefab.transform.localPosition = new Vector3(0, 0f, -0.1f);
                    popupPrefab.transform.localEulerAngles = new Vector3(0, 180, 0);
                    await popupPrefab.WaitUntilInitialized();
                    popupPrefab.WebView.CloseRequested += (popupWebView, closeEventArgs) => {
                        Debug.Log("Closing the popup");
                        popupPrefab.Destroy();
                    };
                };
            }
        }
        
        _webViewPrefab.WebView.LoadUrl("https://webview-popup-tester.vercel.app/");
    }
}
