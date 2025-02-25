//
//  QUICClient.h
//  QUICClient
//
//  Created by Junior Silva (EXT) on 20/01/25.
//

#import <Foundation/Foundation.h>

@interface FrameworkQUICClient : NSObject

+ (instancetype)shared;
- (NSString *)disconnect;

@end

//! Project version number for QUICClient.
FOUNDATION_EXPORT double QUICClientVersionNumber;

//! Project version string for QUICClient.
FOUNDATION_EXPORT const unsigned char QUICClientVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <QUICClient/PublicHeader.h>
