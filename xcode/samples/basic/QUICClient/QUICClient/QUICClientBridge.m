//
//  QUICClientBridge.m
//  QUICClient
//
//  Created by Junior Silva (EXT) on 19/12/24.
//

#include "QUICClient-Swift.h"
#include <stdlib.h>

// Função auxiliar para lidar com NSString para C string (UTF-8)
static char* copyNSStringToCString(NSString *string) {
    return string ? strdup([string UTF8String]) : NULL;
}

// Conecta ao servidor QUIC
void connectToQUIC(const char *host, unsigned short port, void (*completionHandler)(const char *)) {
    FrameworkQUICClient *client = [FrameworkQUICClient shared];
    NSString *hostString = host ? [NSString stringWithUTF8String:host] : @"www.google.com";

    [client connectToQUICWithHost:hostString
                             port:port
                completionHandler:^(NSString *result) {
        if (completionHandler) {
            const char *cResult = [result UTF8String];
            completionHandler(cResult);
        }
    }];
}

// Faz uma requisição GET para o servidor QUIC
void getRequestToServer(const char *route, void (*completionHandler)(const char *)) {
    FrameworkQUICClient *client = [FrameworkQUICClient shared];
    NSString *routeString = route ? [NSString stringWithUTF8String:route] : @"/search?q=WildlifeStudios&tbm=nws";

    [client sendGetRequestWithRoute:routeString
                  completionHandler:^(NSString *result) {
        if (completionHandler) {
            const char *cResult = [result UTF8String];
            completionHandler(cResult);
        }
    }];
}

// Desconecta do servidor QUIC
char* disconnectFromQUIC(void) {
    FrameworkQUICClient *client = [FrameworkQUICClient shared];
    NSString *result = [client disconnectFromQUIC];
    return result ? strdup([result UTF8String]) : NULL;
}
