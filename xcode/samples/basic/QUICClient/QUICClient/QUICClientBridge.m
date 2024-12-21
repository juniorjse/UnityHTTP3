//
//  QUICClientBridge.m
//  QUICClient
//
//  Created by Junior Silva (EXT) on 19/12/24.
//
#import "QUICClient-Swift.h"
#include <stdlib.h>

char* connectToQUIC(void) {
    __block char *resultCString = NULL;
    
    // Chama o método assíncrono com callback
    [[FrameworkQUICClient shared] connectToQUICWithCompletion:^(NSString *stateMessage) {
        if (stateMessage) {
            resultCString = strdup([stateMessage UTF8String]);
        }
    }];
    
    return resultCString;
}

char* disconnectFromQUIC(void) {
    NSString *result = [[FrameworkQUICClient shared] disconnectFromQUIC];
    return result ? strdup([result UTF8String]) : NULL;
}

char* getRequestToServer(void) {
    __block char *resultCString = NULL;

    // Chama o método assíncrono com callback
    [[FrameworkQUICClient shared] getRequestToServerWithCompletion:^(NSString *response) {
        if (response) {
            resultCString = strdup([response UTF8String]);
        }
    }];

    return resultCString;
}
