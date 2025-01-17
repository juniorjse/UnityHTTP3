////
////  quicview.swift
////  QUICClient
////
////  Created by Junior Silva (EXT) on 25/11/24.
////
//
//import SwiftUI
//
//struct QuicView: View {
//    @State private var result: String = ""
//    @State private var host: String = "www.google.com"
//    @State private var port: UInt16 = 443
//    @State private var route: String = "/search?q=WildlifeStudios&tbm=nws"
//    let quicClient = FrameworkQUICClient.shared
//
//    var body: some View {
//        VStack(spacing: 20) {
//            Text("QUIC Client Test")
//                .font(.headline)
//                .padding()
//
//            VStack(alignment: .leading) {
//                Text("Host:")
//                TextField("Host", text: $host)
//                    .textFieldStyle(RoundedBorderTextFieldStyle())
//                    .padding(.bottom, 10)
//
//                Text("Port:")
//                TextField("Port", value: $port, formatter: NumberFormatter())
//                    .textFieldStyle(RoundedBorderTextFieldStyle())
//                    .padding(.bottom, 10)
//
//                Text("Route:")
//                TextField("Route", text: $route)
//                    .textFieldStyle(RoundedBorderTextFieldStyle())
//            }
//            .padding()
//
//            Button("Connect") {
//                quicClient.connectQUIC(host: host, port: port) { connectionResult in
//                    DispatchQueue.main.async {
//                        self.result = connectionResult
//                    }
//                }
//            }
//            .buttonStyle(.borderedProminent)
//            .padding()
//
//            Button("Request") {
//                quicClient.sendGetRequest(route: route) { requestResult in
//                    DispatchQueue.main.async {
//                        self.result = requestResult
//                    }
//                }
//            }
//            .buttonStyle(.borderedProminent)
//            .padding()
//
//            Button("Disconnect") {
//                DispatchQueue.main.async {
//                    self.result = quicClient.disconnectFromQUIC()
//                }
//            }
//            .buttonStyle(.borderedProminent)
//            .padding()
//
//            Text("Result:")
//                .font(.subheadline)
//                .padding(.top)
//
//            ScrollView {
//                Text(result)
//                    .padding()
//                    .foregroundColor(.blue)
//                    .multilineTextAlignment(.leading)
//            }
//            .frame(maxHeight: 200)
//        }
//        .padding()
//    }
//}
//
//#Preview {
//    QuicView()
//}
