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

    @objc public override init() {
        super.init()
    }

    @objc public func connectToQUIC(completionHandler: @escaping (String) -> Void) {
        if let connection = connection, connection.state == .ready {
            completionHandler("Already connected.")
            return
        }

        let host = "www.google.com"
        let port = 443
        let quicOptions = NWProtocolQUIC.Options()
        let secOptions = quicOptions.securityProtocolOptions
        sec_protocol_options_add_tls_application_protocol(secOptions, "h3")
        let parameters = NWParameters(quic: quicOptions)
        let endpoint = NWEndpoint.hostPort(host: NWEndpoint.Host(host), port: NWEndpoint.Port(rawValue: UInt16(port))!)
        connection = NWConnection(to: endpoint, using: parameters)

        connection?.stateUpdateHandler = { state in
            let result: String
            switch state {
            case .ready:
                result = "Connected to \(host):\(port)"
            case .failed(let error):
                result = "Connection failed: \(error.localizedDescription)"
            case .waiting(let error):
                result = "Waiting: \(error.localizedDescription)"
            case .preparing:
                result = "Preparing to connect..."
            default:
                result = "Disconnected"
            }
            DispatchQueue.main.async {
                completionHandler(result)
            }
        }

        connection?.start(queue: .global())
    }

    @objc public func getRequestToServer(completionHandler: @escaping (String) -> Void) {
        guard let connection = connection, connection.state == .ready else {
            completionHandler("No active connection. Please connect first.")
            return
        }

        let url = "https://www.google.com/search?q=WildlifeStudios&tbm=nws"
        guard let requestUrl = URL(string: url) else {
            DispatchQueue.main.async {
                completionHandler("Invalid URL")
            }
            return
        }

        var request = URLRequest(url: requestUrl)
        request.httpMethod = "GET"

        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            let result: String
            if let error = error {
                result = "Request failed: \(error.localizedDescription)"
            } else if let data = data,
                      let htmlString = String(data: data, encoding: .utf8) ?? String(data: data, encoding: .isoLatin1) {
                result = "Response: \(htmlString.prefix(300))..."
            } else {
                result = "No data received"
            }

            DispatchQueue.main.async {
                completionHandler(result)
            }
        }

        task.resume()
    }

    @objc public func disconnectFromQUIC() -> String {
        guard let connection = connection else {
            return "No active connection to disconnect."
        }

        if connection.state != .ready {
            return "Connection is not active. Cannot disconnect."
        }

        connection.cancel()
        self.connection = nil
        let message = "Disconnected successfully."
        print(message)
        return message
    }

}
