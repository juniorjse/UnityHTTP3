#include "QUICClient-Swift.h"
#include <stdlib.h>

// Método síncrono para connectToQUIC
char* connectToQUIC(void) {
    NSString *result = [[FrameworkQUICClient shared] connectToQUIC];
    return result ? strdup([result UTF8String]) : NULL;
}

// Método síncrono para getRequestToServer
char* getRequestToServer(void) {
    NSString *result = [[FrameworkQUICClient shared] getRequestToServer];
    return result ? strdup([result UTF8String]) : NULL;
}

// Método síncrono para disconnectFromQUIC
char* disconnectFromQUIC(void) {
    NSString *result = [[FrameworkQUICClient shared] disconnectFromQUIC];
    return result ? strdup([result UTF8String]) : NULL;
}
