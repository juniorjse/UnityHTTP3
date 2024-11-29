//
//  ResponseView.swift
//  quic-client
//
//  Created by Junior Silva (EXT) on 19/11/24.
//

import SwiftUI

struct ResponseView: View {
    let response: String
    let searchQuery: String
    @Environment(\.dismiss) private var dismiss

    var body: some View {
        VStack {
            HStack {
                Button(action: { dismiss() }) {
                    HStack {
                        Image(systemName: "arrow.left")
                            .foregroundColor(.blue)
                        Text("Back")
                            .foregroundColor(.blue)
                    }
                }
                Spacer()
            }
            .padding()

            Text("\(searchQuery)")
                .font(.largeTitle)
                .fontWeight(.bold)
                .padding(.top, 20)

            ScrollView {
                Text(response)
                    .padding()
                    .frame(maxWidth: .infinity, alignment: .leading)
            }
        }
        .navigationBarHidden(true)
    }
}
