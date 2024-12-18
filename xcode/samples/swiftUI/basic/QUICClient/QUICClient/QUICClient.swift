//  QUICClient.swift
//  QUICClient
//
//  Created by Junior Silva (EXT) on 29/11/24.
//
import Foundation
import Network

@objc public class FrameworkQUICClient: NSObject {
    
    @objc public override init() {
        super.init()
    }
    
    @objc public func connectToQUIC() -> Void{
        let host = "www.google.com"
        let port = 443
        
        let quicOptions = NWProtocolQUIC.Options()
        let secOptions = quicOptions.securityProtocolOptions
        sec_protocol_options_add_tls_application_protocol(secOptions, "h3")
        let parameters = NWParameters(quic: quicOptions)
        
        let endpoint = NWEndpoint.hostPort(host: NWEndpoint.Host(host), port: NWEndpoint.Port(rawValue: UInt16(port))!)
        let connection = NWConnection(to: endpoint, using: parameters)
        
        connection.stateUpdateHandler = { state in
            DispatchQueue.main.async {
                switch state {
                case .ready:
                    print("✅ Connected to \(host):\(port)")
                case .failed(let error):
                    print("❌ Connection failed: \(error.localizedDescription)")
                case .waiting(let error):
                    print("⚠️ Waiting: \(error.localizedDescription)")
                case .preparing:
                    print("🔄 Preparing to connect...")
                default:
                    break
                }
            }
        }
        connection.start(queue: .main)
    }
    
    @objc public func disconnectFromQUIC() -> Void{
        print("🔌 Disconnected")
    }
    
    @objc public func getRequestToServer() {
        let url = "https://www.google.com/search?q=WildlifeStudios&tbm=nws"
        guard let requestUrl = URL(string: url) else {
            print("❌ Invalid URL")
            return
        }
        
        var request = URLRequest(url: requestUrl)
        request.httpMethod = "GET"
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                DispatchQueue.main.async {
                    print("❌ Request failed: \(error.localizedDescription)")
                }
                return
            }
            
            guard let data = data else {
                DispatchQueue.main.async {
                    print("❌ No data received")
                }
                return
            }
            
            if let htmlString = String(data: data, encoding: .utf8) ?? String(data: data, encoding: .isoLatin1) {
                DispatchQueue.main.async {
                    print("✅ Response: \(htmlString.prefix(300))...")
                }
            } else {
                DispatchQueue.main.async {
                    print("❌ Unable to decode response")
                }
            }
        }
        
        task.resume()
        print("🌐 GET request sent to \(url)")
    }
}
