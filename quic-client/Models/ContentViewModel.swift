//
//  ContentViewModel.swift
//  quic-client
//
//  Created by Junior Silva (EXT) on 19/11/24.
//

import SwiftUI
import Network
import SwiftSoup // Necessário para fazer o parsing de HTML

class ContentViewModel: ObservableObject {
    @Published var connectionStatus: String = "N/A"
    @Published var protocolUsed: String = "N/A"
    @Published var isConnected: Bool = false
    @Published var response: String = ""
    @Published var searchQuery: String = "Computação UFCG"

    private var connection: NWConnection?

    func connectToQUIC() {
        let quicOptions = NWProtocolQUIC.Options()
        let secOptions = quicOptions.securityProtocolOptions
        sec_protocol_options_add_tls_application_protocol(secOptions, "h3")
        let parameters = NWParameters(quic: quicOptions)
        let endpoint = NWEndpoint.hostPort(host: "www.google.com", port: 443)
        connection = NWConnection(to: endpoint, using: parameters)
        connection?.stateUpdateHandler = { state in
            DispatchQueue.main.async {
                switch state {
                case .ready:
                    self.protocolUsed = "H3"
                    self.connectionStatus = "Connected"
                    self.isConnected = true
                case .failed(let error):
                    self.connectionStatus = "Failed: \(error.localizedDescription)"
                    self.protocolUsed = "N/A"
                    self.isConnected = false
                default:
                    break
                }
            }
        }
        connection?.start(queue: .main)
    }

    func disconnectFromQUIC() {
        connection?.cancel()
        connection = nil
        connectionStatus = "Disconnected"
        protocolUsed = "N/A"
        isConnected = false
    }

    func getRequestToServer() {
        guard let url = URL(string: "https://www.google.com/search?q=\(searchQuery)&tbm=nws") else { return }
        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                DispatchQueue.main.async {
                    self.response = "Error: \(error.localizedDescription)"
                }
                return
            }
            guard let data = data else {
                DispatchQueue.main.async {
                    self.response = "No data received"
                }
                return
            }

            // Parsing HTML with SwiftSoup
            if let isoEncodedString = String(data: data, encoding: .isoLatin1) {
                do {
                    let doc = try SwiftSoup.parse(isoEncodedString)
                    if let firstNewsTitle = try doc.select("h3").first()?.text() {
                        DispatchQueue.main.async {
                            self.response = firstNewsTitle
                        }
                    } else {
                        DispatchQueue.main.async {
                            self.response = "No news title found"
                        }
                    }
                } catch {
                    DispatchQueue.main.async {
                        self.response = "Error parsing HTML"
                    }
                }
            } else {
                DispatchQueue.main.async {
                    self.response = "Unable to decode response as ISO-8859-1"
                }
            }
        }
        task.resume()
    }
}

// Previews
#Preview {
    ContentView()
}
