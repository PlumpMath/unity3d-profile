#import "SoomlaProfile.h"
#import "UserProfile.h"
#import "UserProfileStorage.h"
#import "SocialActionUtils.h"
#import "UserProfileNotFoundException.h"
#import "ProviderNotFoundException.h"
#import "UnityProfileCommons.h"
#import "UnityCommons.h"
#import "Reward.h"
#import "SoomlaUtils.h"
#import "UnityProfileEventDispatcher.h"
#import "NSData-Base64.h"

extern "C"{
	
    void soomlaProfile_Initialize(const char* customParamsJson) {
        LogDebug(@"SOOMLA Unity UnitySoomlaProfile", @"Initializing ProfileEventHandler and SoomlaProfile");
        
        NSString* customParamsJsonS = [NSString stringWithUTF8String:customParamsJson];
        NSDictionary* customParamsDict = [SoomlaUtils jsonStringToDict:customParamsJsonS];
        
        NSMutableDictionary *parsedCustomParams = [NSMutableDictionary dictionary];
        for (NSString* key in customParamsDict) {
            id value = customParamsDict[key];
            [parsedCustomParams setObject:value forKey:@([UserProfileUtils providerStringToEnum:key])];
        }
        
//        [SoomlaProfile usingExternalProvider:YES];
        [UnityProfileEventDispatcher initialize];
        [[SoomlaProfile getInstance] initialize:parsedCustomParams];
    }
    
    void soomlaProfile_Login(const char* sProvider, const char* payload) {
        NSString* providerIdS = [NSString stringWithUTF8String:sProvider];
        NSString* payloadS = [NSString stringWithUTF8String:payload];
        
        [[SoomlaProfile getInstance] loginWithProvider:[UserProfileUtils providerStringToEnum:providerIdS]
                                            andPayload:payloadS andReward:nil];
    }
    
    void soomlaProfile_Logout(const char* sProvider){
        NSString* providerIdS = [NSString stringWithUTF8String:sProvider];
        
        [[SoomlaProfile getInstance] logoutWithProvider:[UserProfileUtils providerStringToEnum:providerIdS]];
    }
    
    bool soomlaProfile_IsLoggedIn(const char* sProvider){
        NSString* providerIdS = [NSString stringWithUTF8String:sProvider];
        
        return [[SoomlaProfile getInstance] isLoggedInWithProvider:[UserProfileUtils providerStringToEnum:providerIdS]];
    }
	
    NSString* soomlaProfile_GetAccessToken(const char* sProvider){
        NSString* providerIdS = [NSString stringWithUTF8String:sProvider];
        
        return [[SoomlaProfile getInstance] getAccessTokenWithProvider:[UserProfileUtils providerStringToEnum:providerIdS]];
    }
    
    void soomlaProfile_UpdateStatus(const char* sProvider, const char* status, const char* payload){
        NSString* providerIdS = [NSString stringWithUTF8String:sProvider];
        NSString* statusS = [NSString stringWithUTF8String:status];
        NSString* payloadS = [NSString stringWithUTF8String:payload];
        
        [[SoomlaProfile getInstance] updateStatusWithProvider:[UserProfileUtils providerStringToEnum:providerIdS]
                                                    andStatus:statusS andPayload:payloadS andReward:nil];
    }
    
    void soomlaProfile_UpdateStory(const char* sProvider, const char* message, const char* name,
                                   const char* caption, const char* description,
                                   const char* link, const char* pictureUrl, const char* payload){
        NSString* providerIdS = [NSString stringWithUTF8String:sProvider];
        NSString* messageS = [NSString stringWithUTF8String:message];
        NSString* nameS = [NSString stringWithUTF8String:name];
        NSString* captionS = [NSString stringWithUTF8String:caption];
        NSString* descriptionS = [NSString stringWithUTF8String:description];
        NSString* linkS = [NSString stringWithUTF8String:link];
        NSString* pictureUrlS = [NSString stringWithUTF8String:pictureUrl];
        NSString* payloadS = [NSString stringWithUTF8String:payload];
        
        [[SoomlaProfile getInstance] updateStoryWithProvider:[UserProfileUtils providerStringToEnum:providerIdS]
                                                  andMessage:messageS andName:nameS andCaption:captionS
                                              andDescription:descriptionS andLink:linkS andPicture:pictureUrlS
                                                  andPayload:payloadS andReward:nil];
    }
    
    void soomlaProfile_UploadImage(const char* sProvider, const char* message, const char* fileName, const char *imageBase64Str, const char* payload){
        NSString* providerIdS = [NSString stringWithUTF8String:sProvider];
        NSString* messageS = [NSString stringWithUTF8String:message];
        NSString* fileNameS = [NSString stringWithUTF8String:fileName];
        NSString* imageBase64StrS = [NSString stringWithUTF8String:imageBase64Str];
        NSString* payloadS = [NSString stringWithUTF8String:payload];
        
        NSData *imageData = [NSData dataFromBase64String:imageBase64StrS];
        [[SoomlaProfile getInstance] uploadImageWithProvider:[UserProfileUtils providerStringToEnum:providerIdS] andMessage:messageS andImageFileName:fileNameS andImageData:imageData andPayload:payloadS andReward:nil];
    }
    
    void soomlaProfile_GetContacts(const char* sProvider, const char* payload){
        NSString* providerIdS = [NSString stringWithUTF8String:sProvider];
        NSString* payloadS = [NSString stringWithUTF8String:payload];
        
        [[SoomlaProfile getInstance] getContactsWithProvider:[UserProfileUtils providerStringToEnum:providerIdS]
                                                  andPayload:payloadS andReward:nil];
    }
    
	int soomlaProfile_GetStoredUserProfile(const char* sProvider, char** json) {
        NSLog(@"SOOMLA/UNITY soomlaProfile_GetStoredUserProfile");
        NSString* providerIdS = [NSString stringWithUTF8String:sProvider];
		@try {
            Provider provider = [UserProfileUtils providerStringToEnum:providerIdS];
            UserProfile* userProfile = nil;
            if ([SoomlaProfile isUsingExternalProvider]) {
                NSLog(@"SOOMLA/UNITY isUsingExternalProvider[true]");
                userProfile = [UserProfileStorage getUserProfile:provider];
                NSLog(@"SOOMLA/UNITY userProfile:%@", [userProfile debugDescription]);
                if (!userProfile) {
                    return EXCEPTION_USER_PROFILE_NOT_FOUND;
                }
            }
            else {
                NSLog(@"SOOMLA/UNITY isUsingExternalProvider[false]");
                userProfile = [[SoomlaProfile getInstance] getStoredUserProfileWithProvider:provider];
            }
            
            NSDictionary* userDict = [userProfile toDictionary];
            NSLog(@"SOOMLA/UNITY userDict:%@", userDict);
            NSString* userProfileJson = [SoomlaUtils dictToJsonString:userDict];
            *json = Soom_AutonomousStringCopy([userProfileJson UTF8String]);
		}
		
		@catch (ProviderNotFoundException* e) {
            NSLog(@"SOOMLA/UNITY Couldn't find a Provider with providerId: %@.", providerIdS);
			return EXCEPTION_PROVIDER_NOT_FOUND;
        }
        @catch (UserProfileNotFoundException* e) {
            NSLog(@"SOOMLA/UNITY Couldn't find a UserProfile for providerId %@.", providerIdS);
			return EXCEPTION_USER_PROFILE_NOT_FOUND;
        }

		return NO_ERR;
	}
    
    void soomlaProfile_SetStoredUserProfile(const char* json, BOOL notify) {
        NSString* userProfileJson = [NSString stringWithUTF8String:json];
        UserProfile* userProfile = [[UserProfile alloc] initWithDictionary:[SoomlaUtils jsonStringToDict:userProfileJson]];
        [UserProfileStorage setUserProfile:userProfile andNotify:notify];
	}
    
    void soomlaProfile_RemoveUserProfile(const char* json) {
        NSLog(@"SOOMLA/UNITY soomlaProfile_RemoveUserProfile");
        NSString* userProfileJson = [NSString stringWithUTF8String:json];
        UserProfile* userProfile = [[UserProfile alloc] initWithDictionary:[SoomlaUtils jsonStringToDict:userProfileJson]];
        [UserProfileStorage removeUserProfile:userProfile];
    }
	   
    void soomlaProfile_OpenAppRatingPage() {
        [[SoomlaProfile getInstance] openAppRatingPage];
    }
    
}