//
//  QUICClientBridge.m
//  QUICClient
//
//  Created by Junior Silva (EXT) on 19/12/24.
//
#import <Foundation/Foundation.h>
#import "QUICClient-Swift.h"

void connectToQUIC(void) {
    FrameworkQUICClient *client = [[FrameworkQUICClient alloc] init];
    [client connectToQUIC];
}

void disconnectFromQUIC(void) {
    FrameworkQUICClient *client = [[FrameworkQUICClient alloc] init];
    [client disconnectFromQUIC];
}

void getRequestToServer(void) {
    FrameworkQUICClient *client = [[FrameworkQUICClient alloc] init];
    [client getRequestToServer];
}
