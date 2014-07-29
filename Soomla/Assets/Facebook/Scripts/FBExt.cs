﻿using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Facebook.MiniJSON;

using Soomla;
using Soomla.Profile;

public sealed partial class FB : ScriptableObject 
{
	public static UserProfile UserProfileFromFBJson(string fbUserJson) {
		JSONObject fbJsonObject = new JSONObject (fbUserJson);
		JSONObject soomlaJsonObject = new JSONObject ();
		soomlaJsonObject.AddField(PJSONConsts.UP_PROVIDER, Provider.FACEBOOK.ToString ());
		soomlaJsonObject.AddField(PJSONConsts.UP_PROFILEID, fbJsonObject["id"].str);
		string name = fbJsonObject ["name"].str;
		soomlaJsonObject.AddField(PJSONConsts.UP_USERNAME, name);
		string email = fbJsonObject ["email"] != null ? fbJsonObject ["email"].str : null;
		if (email == null) {
			email = Regex.Replace(name, @"\s+", ".") + "@facebook.com";
		}
		soomlaJsonObject.AddField(PJSONConsts.UP_EMAIL, email);
		soomlaJsonObject.AddField(PJSONConsts.UP_FIRSTNAME, fbJsonObject["first_name"].str);
		soomlaJsonObject.AddField(PJSONConsts.UP_LASTNAME, fbJsonObject["last_name"].str);
		soomlaJsonObject.AddField(PJSONConsts.UP_AVATAR, fbJsonObject["picture"]["data"]["url"].str);
		UserProfile userProfile = new UserProfile (soomlaJsonObject);
		
		return userProfile;
	}

	private static void ProfileCallback(FBResult result) {
		if (result.Error != null) {
			SoomlaProfile.PushEventLoginFailed (Provider.FACEBOOK, result.Error);			
		} 
		else {
			SoomlaUtils.LogDebug(TAG, "ProfileCallback[result.Text]:"+result.Text);
			SoomlaUtils.LogDebug(TAG, "ProfileCallback[result.Texture]:"+result.Texture);
			string fbUserJson = result.Text;
			UserProfile userProfile = UserProfileFromFBJson(fbUserJson);
			// going via the storage will also fire the login finish event
			SoomlaProfile.StoreUserProfile (userProfile, true);
		}
	}
	
	private static void LoginCallback(FBResult result) {
		if (result.Error != null) {
			SoomlaProfile.PushEventLoginFailed (Provider.FACEBOOK, result.Error);			
		}
		else if (!FB.IsLoggedIn) {
			SoomlaProfile.PushEventLoginCancelled(Provider.FACEBOOK);
		}
		else {
			FB.API("/me/permissions", Facebook.HttpMethod.GET, delegate (FBResult response) {
				// inspect the response and adapt your UI as appropriate
				// check response.Text and response.Error
				SoomlaUtils.LogWarning("SOOMLA FBConsole", "me/permissions" + response.Text);
			});
			
			FB.API("/me?fields=id,name,email,first_name,last_name,picture",
			       Facebook.HttpMethod.GET, ProfileCallback);
		}
	}

	// call FB.Feed(...)
//	public static void UpdateStatus() {
//	}

	// requires user_friends permission (and only works on canvas games?)
	public static void GetFriends(Facebook.FacebookDelegate callback) {
		FB.API ("/me/friends?fields=id,name,picture,email,first_name,last_name",
		        Facebook.HttpMethod.GET,
		        callback);
	}

	// requires read_stream permission
	public static void GetFeed(Facebook.FacebookDelegate callback) {
		FB.API ("/me/feed",
		        Facebook.HttpMethod.GET,
		        callback);
	}
	
	public static void UploadImage(Texture2D tex2D, string fileName, string message, Facebook.FacebookDelegate callback) {
		byte[] texBytes = tex2D.EncodeToPNG();
		
		var wwwForm = new WWWForm();
		wwwForm.AddBinaryData("image", texBytes, fileName);
		wwwForm.AddField("message", message);
		
		FB.API("/me/photos", Facebook.HttpMethod.POST, callback, wwwForm);
	}

	public static void UploadImage(string filePath, string message) {
		// TODO: not as simple, later
	}

	private static void FeedCallback(FBResult result) {
		if (result.Error != null) {
			SoomlaProfile.PushEventSocialActionFailed (Provider.FACEBOOK, SocialActionType.UPDATE_STATUS, result.Error);
		}
		else {
			SoomlaUtils.LogDebug(TAG, "FeedCallback[result.Text]:"+result.Text);
			SoomlaUtils.LogDebug(TAG, "FeedCallback[result.Texture]:"+result.Texture);
			var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;
			object obj = 0;
			if (responseObject.TryGetValue("cancelled", out obj)) {
				SoomlaProfile.PushEventSocialActionCancelled(Provider.FACEBOOK, SocialActionType.UPDATE_STATUS);
			}
			else /*if (responseObject.TryGetValue ("id", out obj))*/ {
				SoomlaProfile.PushEventSocialActionFinished(Provider.FACEBOOK, SocialActionType.UPDATE_STATUS);
			}
		}
	}

	private static void UploadImageCallback(FBResult result) {
		if (result.Error != null) {
			SoomlaUtils.LogDebug(TAG, "UploadImageCallback[result.Error]:"+result.Error);
			SoomlaProfile.PushEventSocialActionFailed (Provider.FACEBOOK, SocialActionType.UPLOAD_IMAGE, result.Error);
		}
		else {
			SoomlaUtils.LogDebug(TAG, "UploadImageCallback[result.Text]:"+result.Text);
			SoomlaUtils.LogDebug(TAG, "UploadImageCallback[result.Texture]:"+result.Texture);
			var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;
			object obj = 0;
			if (responseObject.TryGetValue("cancelled", out obj)) {
				SoomlaProfile.PushEventSocialActionCancelled(Provider.FACEBOOK, SocialActionType.UPLOAD_IMAGE);
			}
			else /*if (responseObject.TryGetValue ("id", out obj))*/ {
				SoomlaProfile.PushEventSocialActionFinished(Provider.FACEBOOK, SocialActionType.UPLOAD_IMAGE);
			}
		}
	}

	private static void GetContactsCallback(FBResult result) {
		if (result.Error != null) {
			SoomlaUtils.LogDebug(TAG, "GetContactsCallback[result.Error]:"+result.Error);
			SoomlaProfile.PushEventSocialActionFailed (Provider.FACEBOOK, SocialActionType.GET_CONTACTS, result.Error);
		}
		else {
			SoomlaUtils.LogDebug(TAG, "GetContactsCallback[result.Text]:"+result.Text);
			SoomlaUtils.LogDebug(TAG, "GetContactsCallback[result.Texture]:"+result.Texture);
			SoomlaProfile.PushEventSocialActionFinished(Provider.FACEBOOK, SocialActionType.GET_CONTACTS);
		}
	}

	private static void AppRequestCallback(FBResult result) {
		// TODO: maybe later track any graph request
		if (result.Error != null) {
			SoomlaUtils.LogDebug(TAG, "AppRequestCallback[result.Error]:"+result.Error);
		}
		else {
			SoomlaUtils.LogDebug(TAG, "AppRequestCallback[result.Text]:"+result.Text);
			SoomlaUtils.LogDebug(TAG, "AppRequestCallback[result.Texture]:"+result.Texture);
			var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;
			object obj = 0;
			if (responseObject.TryGetValue("error", out obj)) {
//				SoomlaProfile.PushEventSocialActionCancelled(Provider.FACEBOOK, SocialActionType.APP_REQUEST);
			}
			else /*if (responseObject.TryGetValue ("to", out obj))*/ {
//				SoomlaProfile.PushEventSocialActionFinished(Provider.FACEBOOK, SocialActionType.APP_REQUEST);
			}
		}
	}

	private static void APICallback(FBResult result) {
		// TODO: maybe later track any graph request
		if (result.Error != null) {
			SoomlaUtils.LogDebug(TAG, "APICallback[result.Error]:"+result.Error);
		}
		else {
			SoomlaUtils.LogDebug(TAG, "APICallback[result.Text]:"+result.Text);
			SoomlaUtils.LogDebug(TAG, "APICallback[result.Texture]:"+result.Texture);
		}
	}
}