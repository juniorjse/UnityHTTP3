//
//  QUICClientBridge.m
//  QUICClient
//
//  Created by Junior Silva (EXT) on 19/12/24.
//

#include "QUICClient-Swift.h"
#include <stdlib.h>

static char* copyNSStringToCString(NSString *string) {
    return string ? strdup([string UTF8String]) : NULL;
}

void connectQUIC(const char *host, unsigned short port, const char *handshakeOpts, void (*completionHandler)(const char *)) {
    FrameworkQUICClient *client = [FrameworkQUICClient shared];
    NSString *hostString = host ? [NSString stringWithUTF8String:host] : @"www.google.com";
    NSString *handshakeOptsString = handshakeOpts ? [NSString stringWithUTF8String:handshakeOpts] : @"";

    [client connectQUICWithHost:hostString
                          port:port
                 handshakeOpts:handshakeOptsString
             completionHandler:^(NSString *result) {
        if (completionHandler) {
            const char *cResult = [result UTF8String];
            completionHandler(cResult);
        }
    }];
}

void sendQUIC(const char *route, int messageType, unsigned int sequenceNumber, const void *data, unsigned int requestUid, int timeout, void (*completionHandler)(const char *)) {
    
    FrameworkQUICClient *client = [FrameworkQUICClient shared];
    NSString *routeString = route ? [NSString stringWithUTF8String:route] : @"/search?q=WildlifeStudios&tbm=nws";
    
    [client sendQUICWithMessageType:messageType
                             route:routeString
                      sequenceNumber:sequenceNumber
                                data:data ? [NSData dataWithBytes:data length:strlen(data)] : nil
                           requestUid:requestUid
                               timeout:timeout
                   completionHandler:^(NSString *result) {
        if (completionHandler) {
            const char *cResult = [result UTF8String];
            completionHandler(cResult);
        }
    }];
}

char* disconnect(void) {
    FrameworkQUICClient *client = [FrameworkQUICClient shared];
    NSString *result = [client disconnect];
    return result ? strdup([result UTF8String]) : NULL;
}
