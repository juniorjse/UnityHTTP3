//  QUICClient.swift
//  QUICClient
//
//  Created by Junior Silva (EXT) on 29/11/24.
//
import Foundation
import Network

@objc public class FrameworkQUICClient: NSObject {

    @objc public static let shared = FrameworkQUICClient()
    private var connection: NWConnection?

    @objc public override init() {
        super.init()
    }
    
    @objc public func connectToQUIC() -> String {
        let host = "www.google.com"
        let port = 443
        
        let quicOptions = NWProtocolQUIC.Options()
        let secOptions = quicOptions.securityProtocolOptions
        sec_protocol_options_add_tls_application_protocol(secOptions, "h3")
        let parameters = NWParameters(quic: quicOptions)
        
        let endpoint = NWEndpoint.hostPort(host: NWEndpoint.Host(host), port: NWEndpoint.Port(rawValue: UInt16(port))!)
        connection = NWConnection(to: endpoint, using: parameters)
        
        var stateMessage = "Connectting"
        connection?.stateUpdateHandler = { state in
            DispatchQueue.main.async {
                switch state {
                case .ready:
                    stateMessage = "Connected to \(host):\(port)"
                    print(stateMessage)
                case .failed(let error):
                    stateMessage = "Connection failed: \(error.localizedDescription)"
                    print(stateMessage)
                case .waiting(let error):
                    stateMessage = "Waiting: \(error.localizedDescription)"
                    print(stateMessage)
                case .preparing:
                    stateMessage = "Preparing to connect..."
                    print(stateMessage)
                default:
                    break
                }
            }
        }
        connection?.start(queue: .main)
        return stateMessage
    }
    
    @objc public func getRequestToServer() -> String {
        let url = "https://www.google.com/search?q=WildlifeStudios&tbm=nws"
        guard let requestUrl = URL(string: url) else {
            let errorMessage = "❌ Invalid URL"
            print(errorMessage)
            return errorMessage
        }
        
        var resultMessage = ""
        var request = URLRequest(url: requestUrl)
        request.httpMethod = "GET"
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                DispatchQueue.main.async {
                    resultMessage = "❌ Request failed: \(error.localizedDescription)"
                    print(resultMessage)
                }
                return
            }
            
            guard let data = data else {
                DispatchQueue.main.async {
                    resultMessage = "❌ No data received"
                    print(resultMessage)
                }
                return
            }
            
            if let htmlString = String(data: data, encoding: .utf8) ?? String(data: data, encoding: .isoLatin1) {
                DispatchQueue.main.async {
                    resultMessage = "✅ Response: \(htmlString.prefix(300))..."
                    print(resultMessage)
                }
            } else {
                DispatchQueue.main.async {
                    resultMessage = "❌ Unable to decode response"
                    print(resultMessage)
                }
            }
        }
        
        task.resume()
        let sentMessage = "🌐 GET request sent to \(url)"
        print(sentMessage)
        return sentMessage
    }

    @objc public func disconnectFromQUIC() -> String {
        connection?.cancel()
        connection = nil
        let message = "Disconnected"
        print(message)
        return message
    }
}
