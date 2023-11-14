using UnityEngine;
using Vuplex.WebView;
using UnityEngine.XR;

class OculusWebViewDemo : MonoBehaviour {

    WebViewPrefab _webViewPrefab;
    Keyboard _keyboard;

    async void Start() {

        // Use a desktop User-Agent to request the desktop versions of websites.
        // https://developer.vuplex.com/webview/Web#SetUserAgent
        Web.SetUserAgent("Mozilla/5.0 (X11; Linux x86_64; Quest Pro) Chrome/112.0.5615.204 AppleWebKit/537.36 (KHTML, like Gecko) Safari/537.36");

        // Create a 0.6 x 0.4 instance of the prefab.
        // https://developer.vuplex.com/webview/WebViewPrefab#Instantiate
        _webViewPrefab = WebViewPrefab.Instantiate(1.2f, 0.8f, new WebViewOptions {
            preferredPlugins = new WebPluginType[] { WebPluginType.Android }
        } );
        _webViewPrefab.transform.SetParent(transform, false);
        _webViewPrefab.transform.localPosition = new Vector3(0, 0.2f, 1.0f);
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

        // After the prefab has initialized, you can use the IWebView APIs via its WebView property.
        // https://developer.vuplex.com/webview/IWebView

        _webViewPrefab.WebView.LoadUrl("https://webview-popup-tester.vercel.app/");
        await _webViewPrefab.WebView.WaitForNextPageLoadToFinish();
        Debug.Log("setting up popups");
        var webViewWithPopups = _webViewPrefab.WebView as IWithPopups;
        if (webViewWithPopups != null) {
            
            Debug.Log("setting up popups in new webview");
            webViewWithPopups.SetPopupMode(PopupMode.LoadInNewWebView);

            webViewWithPopups.PopupRequested += async (sender, eventArgs) => {
                Debug.Log("Popup opened with URL: " + eventArgs.Url);
                // Create and display a new WebViewPrefab for the popup.
                var popupPrefab = WebViewPrefab.Instantiate(eventArgs.WebView);
                popupPrefab.gameObject.SetActive(true);
                popupPrefab.transform.parent = transform;
                popupPrefab.transform.localPosition = new Vector3(0, 1, 1);
                popupPrefab.transform.localEulerAngles = new Vector3(0, 180, 0);
                await popupPrefab.WaitUntilInitialized();
                popupPrefab.WebView.CloseRequested += (popupWebView, closeEventArgs) => {
                    Debug.Log("Closing the popup");
                    popupPrefab.Destroy();
                };
            };
        }
        
    }
}
