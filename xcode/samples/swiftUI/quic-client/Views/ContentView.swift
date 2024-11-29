//
// ContentView.swift
// quic-client
//
// Created by Junior José da Silva on 07/11/24.
//

import SwiftUI

struct ContentView: View {
    @StateObject private var viewModel = ContentViewModel()

    var body: some View {
        NavigationStack {
            VStack {
                Text("QUIC Client 🌐")
                    .font(.largeTitle)
                    .fontWeight(.bold)
                    .padding(.top, 40)
                    .foregroundColor(.blue)

                Spacer()

                StatusBox(connectionStatus: viewModel.connectionStatus, protocolUsed: viewModel.protocolUsed)

                Spacer()

                Button("Start Connection") {
                    viewModel.connectToQUIC()
                }
                .modifier(CustomButtonStyle(color: .blue, isEnabled: true))
                .padding(.horizontal, 40)

                Button("Get Request") {
                    viewModel.getRequestToServer()
                }
                .modifier(CustomButtonStyle(color: .green, isEnabled: viewModel.isConnected))
                .padding(.horizontal, 40)
                .disabled(!viewModel.isConnected)

                Button("Disconnect") {
                    viewModel.disconnectFromQUIC()
                }
                .modifier(CustomButtonStyle(color: .red, isEnabled: viewModel.isConnected))
                .padding(.horizontal, 40)
                .disabled(!viewModel.isConnected)

                Spacer()

                NavigationLink("View Response", destination: ResponseView(response: viewModel.response, searchQuery: viewModel.searchQuery))
                    .modifier(CustomButtonStyle(color: .blue, isEnabled: !viewModel.response.isEmpty))
                    .padding(.horizontal, 40)
                    .disabled(viewModel.response.isEmpty)
            }
        }
    }
}


// Previews
#Preview {
    ContentView()
}

