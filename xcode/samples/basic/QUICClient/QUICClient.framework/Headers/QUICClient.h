//
//  QUICClient.h
//  QUICClient
//
//  Created by Junior Silva (EXT) on 25/11/24.
//

#import <Foundation/Foundation.h>

@interface FrameworkQUICClient : NSObject

+ (instancetype)shared;
- (NSString *)disconnectFromQUIC;
extern void connectToQUIC(void (*completionHandler)(const char *));
extern void getRequestToServer(void (*completionHandler)(const char *));

@end

FOUNDATION_EXPORT double QUICClientVersionNumber;

FOUNDATION_EXPORT const unsigned char QUICClientVersionString[];
