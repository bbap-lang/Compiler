﻿using System.Text;

namespace BBAP.Transpiler;

public class AbapBuilder {
    private readonly StringBuilder _builder = new();

    private bool _newLine = true;
    private int depth;

    public void Append(string str) {
        Intend();
        _builder.Append(str);
    }

    public void Append(char c) {
        Intend();
        _builder.Append(c);
    }

    public void Append(int value) {
        Intend();
        _builder.Append(value);
    }

    public void Append(long value) {
        Intend();
        _builder.Append(value);
    }

    public void AppendLine(string str) {
        Intend();
        _builder.Append(str);
        AppendLine();
    }

    public void AppendLine(char c) {
        Intend();
        _builder.Append(c);
        AppendLine();
    }

    public void AddIntend() {
        depth++;
    }

    public void RemoveIntend() {
        depth--;
    }

    public override string ToString() {
        return _builder.ToString();
    }

    public void AppendLine() {
        _builder.AppendLine();
        _newLine = true;
    }

    private void Intend() {
        if (_newLine) {
            for (int i = 0; i < depth; i++) {
                _builder.Append('\t');
            }

            _newLine = false;
        }
    }
}