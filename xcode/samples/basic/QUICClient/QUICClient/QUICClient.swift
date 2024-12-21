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
    
    @objc public func connectToQUIC(completion: @escaping (NSString) -> Void) {
        let host = "www.google.com"
        let port = 443
        
        let quicOptions = NWProtocolQUIC.Options()
        let secOptions = quicOptions.securityProtocolOptions
        sec_protocol_options_add_tls_application_protocol(secOptions, "h3")
        let parameters = NWParameters(quic: quicOptions)
        
        let endpoint = NWEndpoint.hostPort(host: NWEndpoint.Host(host), port: NWEndpoint.Port(rawValue: UInt16(port))!)
        connection = NWConnection(to: endpoint, using: parameters)
        
        connection?.stateUpdateHandler = { state in
            DispatchQueue.main.async {
                var stateMessage: String
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
                    stateMessage = "Unknown state"
                }
                // Passa o estado atualizado pelo callback
                completion(stateMessage as NSString)
            }
        }
        connection?.start(queue: .main)
    }
    
    @objc public func getRequestToServer(completion: @escaping (NSString) -> Void) {
        let url = "https://www.google.com/search?q=WildlifeStudios&tbm=nws"
        guard let requestUrl = URL(string: url) else {
            let errorMessage = "❌ Invalid URL"
            print(errorMessage)
            completion(errorMessage as NSString)
            return
        }
        
        var request = URLRequest(url: requestUrl)
        request.httpMethod = "GET"
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                DispatchQueue.main.async {
                    let resultMessage = "❌ Request failed: \(error.localizedDescription)"
                    print(resultMessage)
                    completion(resultMessage as NSString)
                }
                return
            }
            
            guard let data = data else {
                DispatchQueue.main.async {
                    let resultMessage = "❌ No data received"
                    print(resultMessage)
                    completion(resultMessage as NSString)
                }
                return
            }
            
            if let htmlString = String(data: data, encoding: .utf8) ?? String(data: data, encoding: .isoLatin1) {
                DispatchQueue.main.async {
                    let resultMessage = "✅ Response: \(htmlString.prefix(300))..."
                    print(resultMessage)
                    completion(resultMessage as NSString)
                }
            } else {
                DispatchQueue.main.async {
                    let resultMessage = "❌ Unable to decode response"
                    print(resultMessage)
                    completion(resultMessage as NSString)
                }
            }
        }
        
        task.resume()
    }


    @objc public func disconnectFromQUIC() -> String {
        connection?.cancel()
        connection = nil
        let message = "Disconnected"
        print(message)
        return message
    }
}
