import SwiftUI

struct quicView: View {
    @State private var result: String = ""
    let quicClient = FrameworkQUICClient.shared

    var body: some View {
        VStack {
            Button("Connect") {
                result = quicClient.connectToQUIC()
            }
            .padding()

            Button("Request") {
                result = quicClient.getRequestToServer()
            }
            .padding()

            Button("Disconnect") {
                result = quicClient.disconnectFromQUIC()
            }
            .padding()

            Text("Result: \(result)")
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
