//
//  QUICClientBridge.m
//  QUICClient
//
//  Created by Junior Silva (EXT) on 19/12/24.
//
#import "QUICClient-Swift.h"
#include <stdlib.h>

char* connectToQUIC(void) {
    NSString *result = [[FrameworkQUICClient shared] connectToQUIC];
    return result ? strdup([result UTF8String]) : NULL;
}

char* disconnectFromQUIC(void) {
    NSString *result = [[FrameworkQUICClient shared] disconnectFromQUIC];
    return result ? strdup([result UTF8String]) : NULL;
}

char* getRequestToServer(void) {
    NSString *result = [[FrameworkQUICClient shared] getRequestToServer];
    return result ? strdup([result UTF8String]) : NULL;
}
