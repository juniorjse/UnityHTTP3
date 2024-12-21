import SwiftUI

struct quicView: View {
    @State private var result: String = "Press the button to fetch data"
    let quicClient = FrameworkQUICClient.shared

    var body: some View {
        VStack {
            Button("Fetch Data") {
                quicClient.getRequestToServer { response in
                    DispatchQueue.main.async {
                        self.result = response as String
                    }
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
