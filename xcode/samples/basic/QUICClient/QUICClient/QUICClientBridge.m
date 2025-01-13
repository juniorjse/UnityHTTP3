//
//  QUICClientBridge.m
//  QUICClient
//
//  Created by Junior Silva (EXT) on 19/12/24.
//

#include "QUICClient-Swift.h"
#include <stdlib.h>

void connectToQUIC(void (*completionHandler)(const char *)) {
    FrameworkQUICClient *client = [FrameworkQUICClient shared];
    
    [client connectToQUICWithCompletionHandler:^(NSString *result) {
        if (completionHandler) {
            const char *cResult = [result UTF8String];
            completionHandler(cResult);
        }
    }];
}

void getRequestToServer(void (*completionHandler)(const char *)) {
    FrameworkQUICClient *client = [FrameworkQUICClient shared];

    [client getRequestToServerWithCompletionHandler:^(NSString *result) {
        if (completionHandler) {
            const char *cResult = [result UTF8String];
            completionHandler(cResult);
        }
    }];
}

char* disconnectFromQUIC(void) {
    NSString *result = [[FrameworkQUICClient shared] disconnectFromQUIC];
    return result ? strdup([result UTF8String]) : NULL;
}
