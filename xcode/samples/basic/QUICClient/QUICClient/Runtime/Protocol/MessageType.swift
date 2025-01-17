//
//  MessageType.swift
//  QUICClient
//
//  Created by Junior Silva (EXT) on 17/01/25.
//

@objc public enum MessageType: Int {
    case request = 0
    case notify = 1
    case response = 2
    case push = 3
}
