import SwiftUI

struct quicView: View {
    @State private var result: String = ""
    let quicClient = FrameworkQUICClient.shared

    var body: some View {
        VStack {
            Button("Connect") {
                DispatchQueue.main.async {
                    // Chamada síncrona do connectToQUIC
                    self.result = quicClient.connectToQUIC()
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
                    // Chamada síncrona do disconnectFromQUIC
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

// Preview para testar a visualização no SwiftUI Canvas
#Preview {
    quicView()
}
