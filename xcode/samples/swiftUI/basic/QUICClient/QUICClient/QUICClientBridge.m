//
//  QUICClientBridge.m
//  QUICClient
//
//  Created by Junior Silva (EXT) on 19/12/24.
//
#import "QUICClient-Swift.h"

void connectToQUIC(void) {
    [[FrameworkQUICClient shared] connectToQUIC];
}

void disconnectFromQUIC(void) {
    [[FrameworkQUICClient shared] disconnectFromQUIC];
}

void getRequestToServer(void) {
    [[FrameworkQUICClient shared] getRequestToServer];
}