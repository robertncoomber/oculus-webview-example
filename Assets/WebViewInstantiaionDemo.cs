using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuplex.WebView;

public class WebViewInstantiaionDemo : MonoBehaviour
{
    public GameObject _WebViewPrefab;
    private string GlobalUserAgent = "Mozilla/5.0 (X11; Linux x86_64; Quest Pro) Chrome/112.0.5615.204 AppleWebKit/537.36 (KHTML, like Gecko) Safari/537.36";
    [SerializeField] private OVROverlay ovrOverlay;
    
    private async void Awake()
    {
       #if UNITY_ANDROID && !UNITY_EDITOR
           Web.SetUserAgent(GlobalUserAgent);
           Web.SetCameraAndMicrophoneEnabled(true);
       #endif
       #if UNITY_ANDROID && !UNITY_EDITOR
           AndroidWebView.SetDrmEnabled(true);
           AndroidGeckoWebView.SetDrmEnabled(true);
        #endif 
        
        GameObject _webViewPrefabGO = Instantiate(_WebViewPrefab, new Vector3(0.5f, 0.5f, 1.0f), Quaternion.Euler(0, 180, 0), null);
        _webViewPrefabGO.SetActive(true);
        
        WebViewPrefab webViewPrefab = _webViewPrefabGO.GetComponent<WebViewPrefab>();
        webViewPrefab.SetOptionsForInitialization(new WebViewOptions
        {
            preferredPlugins = new WebPluginType[] { WebPluginType.Android }
        });
        webViewPrefab.HoveringEnabled = true;
        
        await webViewPrefab.WaitUntilInitialized();
        
        var surface = ovrOverlay.externalSurfaceObject;
        // webViewPrefab.Resize(ovrOverlay.externalSurfaceWidth, ovrOverlay.externalSurfaceHeight);
        #if UNITY_ANDROID && !UNITY_EDITOR
            var androidWebView = webViewPrefab.WebView as AndroidWebView;
            androidWebView.SetSurface(surface);
        #endif
        
        Debug.Log($"plugin type: {webViewPrefab.WebView.PluginType}");
        
        webViewPrefab.WebView.LoadUrl("https://webview-popup-tester.vercel.app/");
                await webViewPrefab.WebView.WaitForNextPageLoadToFinish();
                Debug.Log("setting up popups");
                var webViewWithPopups = webViewPrefab.WebView as IWithPopups;
                if (webViewWithPopups != null) {
                    
                    Debug.Log("setting up popups in new webview");
                    webViewWithPopups.SetPopupMode(PopupMode.LoadInNewWebView);
        
                    webViewWithPopups.PopupRequested += async (sender, eventArgs) => {
                        Debug.Log("Popup opened with URL: " + eventArgs.Url);
                        // Create and display a new WebViewPrefab for the popup.
                        
                        GameObject _webViewPrefabPopupGO = Instantiate(_WebViewPrefab, new Vector3(0, 1f, 2.0f), Quaternion.Euler(0, 180, 0));
                        
                        Debug.Log("Popup webview instantiated");
                        
                        _webViewPrefabGO.SetActive(true);
                        
                        WebViewPrefab popupPrefab = _webViewPrefabPopupGO.GetComponent<WebViewPrefab>();
                        popupPrefab.SetOptionsForInitialization(new WebViewOptions
                               {
                                   preferredPlugins = new WebPluginType[] { WebPluginType.Android }
                               });
                        popupPrefab.SetWebViewForInitialization(eventArgs.WebView);
                        popupPrefab.HoveringEnabled = true;

                        Debug.Log("Popup webview initialization options added");
                        
                        await popupPrefab.WaitUntilInitialized();
                        
                        Debug.Log("Popup webview initialized");
                        popupPrefab.WebView.CloseRequested += (popupWebView, closeEventArgs) => {
                            Debug.Log("Closing the popup");
                            popupPrefab.Destroy();
                        };
                    };
                }
                
        
    }
}
