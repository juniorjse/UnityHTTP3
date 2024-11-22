//
//  Styles.swift
//  quic-client
//
//  Created by Junior Silva (EXT) on 19/11/24.
//

import SwiftUI

// MARK: - Custom Background
struct CustomBackground: ViewModifier {
    func body(content: Content) -> some View {
        content
            .padding()
            .frame(maxWidth: .infinity)
            .background(Color(.systemGray6))
            .cornerRadius(15)
            .shadow(radius: 5)
    }
}

// MARK: - Button Style
struct CustomButtonStyle: ViewModifier {
    var color: Color
    var isEnabled: Bool

    func body(content: Content) -> some View {
        content
            .font(.headline)
            .foregroundColor(.white)
            .padding()
            .frame(maxWidth: .infinity)
            .background(isEnabled ? color : Color.gray)
            .cornerRadius(10)
            .shadow(radius: 5)
            .opacity(isEnabled ? 1.0 : 0.6)
    }
}

// MARK: - Status Text Component
struct StatusText: View {
    let label: String
    let value: String

    var body: some View {
        HStack {
            Text("\(label):")
                .font(.headline)
                .foregroundColor(.secondary)
            Text(value)
                .font(.body)
                .foregroundColor(.primary)
        }
    }
}

