//
//  QUICClient.swift
//  QUICClient
//
//  Created by Junior Silva (EXT) on 25/11/24.
//

import Foundation
import Network

@objc public class FrameworkQUICClient: NSObject {

    @objc public static let shared = FrameworkQUICClient()
    private var connection: NWConnection?
    private let connectionQueue = DispatchQueue(label: "com.framework.quic.connection", qos: .userInitiated)
    private var isConnected = false

    @objc public override init() {
        super.init()
    }

    @objc public func connectQUIC(host: String = "www.google.com", port: UInt16 = 443, handshakeOpts: String, completionHandler: @escaping (String) -> Void) {
        connectionQueue.async { [weak self] in
            guard let self = self else { return }
            
            if self.isConnected {
                DispatchQueue.main.async {
                    completionHandler("Already connected.")
                }
                return
            }
            
            let quicOptions = NWProtocolQUIC.Options()
            let secOptions = quicOptions.securityProtocolOptions
            sec_protocol_options_add_tls_application_protocol(secOptions, "h3")
            
            let parameters = NWParameters(quic: quicOptions)
            let endpoint = NWEndpoint.hostPort(host: NWEndpoint.Host(host), port: NWEndpoint.Port(rawValue: port)!)
            
            self.connection = NWConnection(to: endpoint, using: parameters)
            self.connection?.stateUpdateHandler = { state in
                let result: String
                switch state {
                case .ready:
                    self.isConnected = true
                    result = "Connected to \(host):\(port)"
                case .failed(let error):
                    self.isConnected = false
                    result = "Connection failed: \(error.localizedDescription)"
                case .waiting(let error):
                    self.isConnected = false
                    result = "Waiting: \(error.localizedDescription)"
                case .preparing:
                    result = "Preparing to connect..."
                default:
                    self.isConnected = false
                    result = "Disconnected"
                }
                DispatchQueue.main.async {
                    completionHandler(result)
                }
            }
            
            self.connection?.start(queue: self.connectionQueue)
        }
    }

    @objc public func sendQUIC(messageType: Int, route: String = "/search?q=WildlifeStudios&tbm=nws", sequenceNumber: UInt = 1, data: Data? = nil, requestUid: UInt = 1, timeout: Int = 60, completionHandler: @escaping (String) -> Void) {
        connectionQueue.async { [weak self] in
            guard let self = self, self.isConnected, let connection = self.connection, connection.state == .ready else {
                DispatchQueue.main.async {
                    completionHandler("No active connection. Please connect first.")
                }
                return
            }

            let url = "https://www.google.com\(route)"
            guard let requestUrl = URL(string: url) else {
                DispatchQueue.main.async {
                    completionHandler("Invalid URL")
                }
                return
            }

            var request = URLRequest(url: requestUrl)
            request.httpMethod = "GET"

            request.addValue("QUICClient/1.0", forHTTPHeaderField: "User-Agent")

            let task = URLSession.shared.dataTask(with: request) { data, response, error in
                var result: String

                if let error = error {
                    result = "Request failed: \(error.localizedDescription)"
                } else if let httpResponse = response as? HTTPURLResponse {
                    if let data = data, !data.isEmpty {
                        if let responseString = String(data: data, encoding: .utf8) ?? String(data: data, encoding: .isoLatin1) {
                            result = "Response (\(httpResponse.statusCode)): \(responseString.prefix(300))..."
                        } else {
                            result = "Response (\(httpResponse.statusCode)): Unable to decode data."
                        }
                    } else {
                        result = "Response (\(httpResponse.statusCode)): No data received."
                    }
                } else {
                    result = "Unknown response format."
                }

                DispatchQueue.main.async {
                    completionHandler(result)
                }
            }

            task.resume()
        }
    }

    @objc public func disconnect() -> String {
        guard let connection = connection else {
            return "No active connection to disconnect."
        }

        if connection.state != .ready {
            return "Connection is not active. Cannot disconnect."
        }

        connection.cancel()
        self.connection = nil
        self.isConnected = false
        let message = "Disconnected successfully."
        print(message)
        return message
    }
}
