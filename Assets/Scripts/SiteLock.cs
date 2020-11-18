using System.Runtime.InteropServices;
using UnityEngine;
public class SiteLock : MonoBehaviour {
    public string [] domains = new string [] {
        "https://www.coolmath-games.com" ,
        "http://www.coolmath-games.com" ,
        "www.coolmath-games.com" ,
        "https://edit.coolmath-games.com" ,
        "http://edit.coolmath-games.com" ,
        "edit.coolmath-games.com" ,
        "www.stage.coolmath-games.com" ,
        "http://www.stage.coolmath-games.com" ,
        "https://www.stage.coolmath-games.com" ,
        "edit-stage.coolmath-games.com" ,
        "http://edit-stage.coolmath-games.com" ,
        "https://edit-stage.coolmath-games.com" ,
        "dev.coolmath-games.com" ,
        "http://dev.coolmath-games.com" ,
        "https://dev.coolmath-games.com" ,
        "m.coolmath-games.com" ,
        "http://m.coolmath-games.com" ,
        "https://m.coolmath-games.com" ,
        "https://www.coolmathgames.com" ,
        "http://www.coolmathgames.com" ,
        "www.coolmathgames.com" ,
        "edit.coolmathgames.com" ,
        "http://edit.coolmathgames.com" ,
        "https://edit.coolmathgames.com" ,
        "www.stage.coolmathgames.com" ,
        "http://www.stage.coolmathgames.com" ,
        "https://www.stage.coolmathgames.com",
        "edit-stage.coolmathgames.com" ,
        "http://edit-stage.coolmathgames.com",
        "https:edit-stage.coolmathgames.com",
        "dev.coolmathgames.com" ,
        "http://dev.coolmathgames.com" ,
        "https://dev.coolmathgames.com" ,
        "m.coolmathgames.com" ,
        "http://m.coolmathgames.com" ,
        "https://m.coolmathgames.com"
    };
    [ DllImport("__Internal") ]
    private static extern void RedirectTo ( string url);
// Check right away if the domain is valid
    private void Start () {
        CheckDomains();
    }
    private void CheckDomains () {
        if (!IsValidHost(domains)) {
            RedirectTo("www.coolmathgames.com");
        }
    }
    private bool IsValidHost ( string [] hosts) {
        foreach ( string host in hosts)
            if (Application.absoluteURL.IndexOf(host) == 0 )
                return true ;
        return false ;
    }
}