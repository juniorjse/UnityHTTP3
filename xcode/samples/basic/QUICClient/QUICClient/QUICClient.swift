import Foundation
import Network

@objc public class FrameworkQUICClient: NSObject {

    @objc public static let shared = FrameworkQUICClient()
    private var connection: NWConnection?

    @objc public override init() {
        super.init()
    }

    // Método para conectar ao QUIC com callback compatível com Objective-C
    @objc public func connectToQUIC() -> String {
        let semaphore = DispatchSemaphore(value: 0)
        var result = "Unknown state"

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
                    result = "Unknown state"
                }
                semaphore.signal()
            }
        }

        connection?.start(queue: .main)
        semaphore.wait()
        return result
    }

    @objc public func getRequestToServer() -> String {
        let semaphore = DispatchSemaphore(value: 0)
        var result = "Unknown result"

        let url = "https://www.google.com/search?q=WildlifeStudios&tbm=nws"
        guard let requestUrl = URL(string: url) else {
            return "❌ Invalid URL"
        }

        var request = URLRequest(url: requestUrl)
        request.httpMethod = "GET"

        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                result = "❌ Request failed: \(error.localizedDescription)"
            } else if let data = data,
                      let htmlString = String(data: data, encoding: .utf8) ?? String(data: data, encoding: .isoLatin1) {
                result = "✅ Response: \(htmlString.prefix(300))..."
            } else {
                result = "❌ No data received"
            }
            semaphore.signal()
        }

        task.resume()
        semaphore.wait()
        return result
    }


    // Método para desconectar do QUIC
    @objc public func disconnectFromQUIC() -> String {
        connection?.cancel()
        connection = nil
        let message = "Disconnected"
        print(message)
        return message
    }
}
