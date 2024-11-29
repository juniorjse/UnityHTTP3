//
//  StatusBox.swift
//  quic-client
//
//  Created by Junior Silva (EXT) on 19/11/24.
//

import SwiftUI

struct StatusBox: View {
    let connectionStatus: String
    let protocolUsed: String

    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            StatusText(label: "Connection Status", value: connectionStatus)
            StatusText(label: "Protocol", value: protocolUsed)
        }
        .padding()
        .frame(maxWidth: .infinity)
        .background(Color(.systemGray6))
        .cornerRadius(15)
        .shadow(radius: 5)
        .padding(.horizontal, 40)
    }
}
