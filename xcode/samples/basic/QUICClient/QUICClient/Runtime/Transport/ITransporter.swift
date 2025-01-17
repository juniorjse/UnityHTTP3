//
//  ITransporter.swift
//  QUICClient
//
//  Created by Junior Silva (EXT) on 17/01/25.
//

import Foundation

@objc public protocol ITransporter {
    func connectQUIC(host: String, port: UInt16, handshakeOpts: String, completionHandler: @escaping (String) -> Void)
    func sendQUIC(messageType: MessageType, route: String, sequenceNumber: UInt, data: Data?, requestUid: UInt, timeout: Int, completionHandler: @escaping (String) -> Void)
    func disconnect() -> String
}

@objc public class Transporter: NSObject, ITransporter {

    private let quicClient = FrameworkQUICClient.shared

    public func connectQUIC(host: String, port: UInt16, handshakeOpts: String, completionHandler: @escaping (String) -> Void) {
        quicClient.connectQUIC(host: host, port: port, handshakeOpts: handshakeOpts, completionHandler: completionHandler)
    }

    public func sendQUIC(messageType: MessageType, route: String, sequenceNumber: UInt, data: Data?, requestUid: UInt, timeout: Int, completionHandler: @escaping (String) -> Void) {
        quicClient.sendQUIC(messageType: messageType.rawValue, route: route, sequenceNumber: sequenceNumber, data: data, requestUid: requestUid, timeout: timeout, completionHandler: completionHandler)
    }

    public func disconnect() -> String {
        return quicClient.disconnect()
    }
}
