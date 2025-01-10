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
//- (void)connectToQUICWithCompletion:(void (^)(const char *result))completionHandler;


@end

//! Project version number for QUICClient.
FOUNDATION_EXPORT double QUICClientVersionNumber;

//! Project version string for QUICClient.
FOUNDATION_EXPORT const unsigned char QUICClientVersionString[];



