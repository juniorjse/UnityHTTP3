//
//  quicview.swift
//  QUICClient
//
//  Created by Junior Silva (EXT) on 25/11/24.
//

import SwiftUI

struct quicView: View {
    @State private var result: String = ""
    let quicClient = FrameworkQUICClient.shared

    var body: some View {
        VStack {
            Button("Connect") {
                quicClient.connectToQUIC { connectionResult in
                    DispatchQueue.main.async {
                        self.result = connectionResult
                    }
                }
            }
            .padding()
            
            Button("Request Data") {
                DispatchQueue.main.async {
                    // Chamada síncrona do getRequestToServer
                    self.result = quicClient.getRequestToServer()
                }
            }
            .padding()

            Button("Disconnect") {
                DispatchQueue.main.async {
                    self.result = quicClient.disconnectFromQUIC()
                }
            }
            .padding()

            Text(result)
                .padding()
                .foregroundColor(.blue)
                .multilineTextAlignment(.center)
        }
        .padding()
    }
}

#Preview {
    quicView()
}
