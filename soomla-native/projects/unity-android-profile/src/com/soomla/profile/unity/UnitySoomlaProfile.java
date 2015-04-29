package com.soomla.profile.unity;

import android.app.Activity;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.util.Base64;

import com.soomla.SoomlaUtils;
import com.soomla.profile.SoomlaProfile;
import com.soomla.profile.data.UserProfileStorage;
import com.soomla.profile.domain.IProvider;
import com.soomla.profile.domain.UserProfile;
import com.soomla.profile.exceptions.ProviderNotFoundException;
import com.soomla.profile.exceptions.UserProfileNotFoundException;

import org.json.JSONException;
import org.json.JSONObject;

import java.security.*;
import java.util.HashMap;
import java.util.Iterator;

import static com.soomla.profile.domain.IProvider.Provider;

public class UnitySoomlaProfile {

    public static void initialize(String customParamsJson) throws JSONException {
        SoomlaUtils.LogDebug(TAG, "Initializing SoomlaProfile from bridge");
        JSONObject customParamsJsonObj = new JSONObject(customParamsJson);
        SoomlaProfile.getInstance().initialize(parseProviderParams(customParamsJsonObj));
    }

    public static void login(Activity activity, String providerStr, String payload) throws ProviderNotFoundException {
        Provider provider = Provider.getEnum(providerStr);
        SoomlaProfile.getInstance().login(activity, provider, payload, null);
    }

    public static void logout(String providerStr) throws ProviderNotFoundException {
        Provider provider = Provider.getEnum(providerStr);
        SoomlaProfile.getInstance().logout(provider);
    }

    public static boolean isLoggedIn(Activity activity, String providerStr) throws ProviderNotFoundException {
        Provider provider = Provider.getEnum(providerStr);
        return SoomlaProfile.getInstance().isLoggedIn(activity, provider);
    }

    public static String getAccessToken(Activity activity, String providerStr) throws ProviderNotFoundException {
        Provider provider = Provider.getEnum(providerStr);
        return SoomlaProfile.getInstance().getAccessToken(activity, provider);
    }

    public static void updateStatus(String providerStr, String status, String payload) throws ProviderNotFoundException {
        Provider provider = Provider.getEnum(providerStr);
        SoomlaProfile.getInstance().updateStatus(provider, status, payload, null);
    }

    public static void updateStory(String providerStr, String message, String name,
                                   String caption, String description, String link,
                                   String pictureUrl, String payload) throws ProviderNotFoundException {
        Provider provider = Provider.getEnum(providerStr);
        SoomlaProfile.getInstance().updateStory(provider, message, name, caption, description,
                link, pictureUrl, payload, null);
    }

    public static void uploadImage(String providerStr, String message, String filePath, String payload) throws ProviderNotFoundException {
        Provider provider = Provider.getEnum(providerStr);
        SoomlaProfile.getInstance().uploadImage(provider, message, filePath, payload, null);
    }

    public static void uploadImage(String providerStr, String message, String fileName, String imageBase64Str,
                                   int jpegQuality, String payload) throws ProviderNotFoundException{
        Provider provider = Provider.getEnum(providerStr);
        byte[] decodedString = Base64.decode(imageBase64Str, Base64.DEFAULT);
        Bitmap imageBitmap = BitmapFactory.decodeByteArray(decodedString, 0, decodedString.length);
        SoomlaProfile.getInstance().uploadImage(provider, message, fileName, imageBitmap, jpegQuality, payload, null);
    }

    public static void getContacts(String providerStr, String payload) throws ProviderNotFoundException {
        Provider provider = Provider.getEnum(providerStr);
        SoomlaProfile.getInstance().getContacts(provider, payload, null);
    }

    public static String getStoredUserProfile(String providerStr) throws ProviderNotFoundException, UserProfileNotFoundException {
        Provider provider = Provider.getEnum(providerStr);
        UserProfile userProfile = SoomlaProfile.getInstance().getStoredUserProfile(provider);
        return userProfile == null ? null : userProfile.toJSONObject().toString();
    }

    public static void storeUserProfile(String userJSON)
            throws ProviderNotFoundException, JSONException {
        JSONObject jsonObject = new JSONObject(userJSON);
        final UserProfile userProfile = new UserProfile(jsonObject);

        UserProfileStorage.setUserProfile(userProfile);
    }

    public static void removeUserProfile(String userJSON)
            throws ProviderNotFoundException, JSONException {
        JSONObject jsonObject = new JSONObject(userJSON);
        final UserProfile userProfile = new UserProfile(jsonObject);

        UserProfileStorage.removeUserProfile(userProfile);
    }

    public static void openAppRatingPage(Activity activity) {
        SoomlaProfile.getInstance().openAppRatingPage(activity.getApplicationContext());
    }

    /*
    * Helper function to retrieve custom params for SoomlaProfile initialization from Json string.
    * @param customParamsJson has the following structure:
    * {"provider1":{"param1":"value1", ... "paramn":"valuen", "provider2": {...}}
    */
    private static HashMap<IProvider.Provider, HashMap<String, String>> parseProviderParams(JSONObject sentParams) {
        if (sentParams == null) {
            SoomlaUtils.LogDebug("SOOMLA", "no provider params were sent");
            return null;
        }

        HashMap<IProvider.Provider, HashMap<String, String>> result = new HashMap<IProvider.Provider, HashMap<String, String>>();
        Iterator keysIterator = sentParams.keys();
        while (keysIterator.hasNext()) {
            String providerStr = (String)keysIterator.next();
            JSONObject paramsEntry = sentParams.optJSONObject(providerStr);

            if (paramsEntry != null) {
                HashMap<String, String> currentProviderParams = new HashMap<String, String>();
                Iterator innerKeysIterator = paramsEntry.keys();
                while (innerKeysIterator.hasNext()) {
                    String innerKey = (String)innerKeysIterator.next();
                    String innerValue = paramsEntry.optString(innerKey);
                    currentProviderParams.put(innerKey, innerValue);
                }

                result.put(IProvider.Provider.getEnum(providerStr), currentProviderParams);
            }
        }

        return result;
    }

    private static String TAG = "SOOMLA UnitySoomlaProfile";
}
