using System.Collections;
using BBAP.Parser.Expressions.Blocks;

namespace BBAP.DefaultClasses; 

public class Stack<T> : IEnumerable<T> {
    private Node? _top = null;
    
    public int Length { get; private set; }
    
    public void Push(T value) {
        var newNode = new Node(value, _top);
        _top = newNode;
        Length++;
    }

    public T Pop() {
        if (_top is null) {
            throw new ArgumentOutOfRangeException();
        }
        
        Node? top = _top;
        
        _top = top.Next;

        Length--;
        return top.Value;
    }

    public T Peek() => Peek(0);
    
    public T Peek(int depth) {
        
        Node? node = _top;

        if (node is null) {
            throw new ArgumentOutOfRangeException();
        }
        
        for (int i = 0; i < depth; i++) {
            node = node.Next;
            if (node is null) {
                throw new ArgumentOutOfRangeException();
            }
        }
        
        return node.Value;
    }
    
    private record Node(T Value, Node? Next);

    public IEnumerator<T> GetEnumerator() {
        Node? node = _top;

        while (node is not null) {
            yield return node.Value;
            
            node = node.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}