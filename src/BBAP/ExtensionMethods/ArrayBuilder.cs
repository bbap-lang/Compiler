using System.Collections.Immutable;

namespace BBAP.ExtensionMethods;

public class ArrayBuilder<T> {
    public static IArrayBuilderBlock<T> Append(T element) {
        return new ArrayBuilderElement<T>(1, null, element);
    }

    public static IArrayBuilderBlock<T> Concat(IReadOnlyCollection<T> collection) {
        return new ArrayBuilderCollection<T>(collection.Count, null, collection);
    }
}

public interface IArrayBuilderBlock<T> {
    public int Length { get; }
    public IArrayBuilderBlock<T> Parent { get; }
}

public static class IArrayBuilderMethods {
    public static IArrayBuilderBlock<T> Append<T>(this IArrayBuilderBlock<T> currentBlock, T element) {
        return new ArrayBuilderElement<T>(1, currentBlock, element);
    }

    public static IArrayBuilderBlock<T> Concat<T>(this IArrayBuilderBlock<T> currentBlock,
        IReadOnlyCollection<T> collection) {
        return new ArrayBuilderCollection<T>(collection.Count, currentBlock, collection);
    }

    public static T[] Build<T>(this IArrayBuilderBlock<T> currentBlock) {
        int length = 0;
        IArrayBuilderBlock<T>? block = currentBlock;
        while (block is not null) {
            length += block.Length;
            block = block.Parent;
        }

        var array = new T[length];
        block = currentBlock;
        int index = length - 1;
        while (block is not null) {
            if (block is ArrayBuilderElement<T> element) {
                array[index] = element.Element;
                index--;
            } else if (block is ArrayBuilderCollection<T> collection) {
                index -= collection.Length - 1;
                foreach (T item in collection.Collection) {
                    array[index] = item;
                    index++;
                }

                index -= collection.Length + 1;
            }

            block = block.Parent;
        }

        return array;
    }

    public static ImmutableArray<T> BuildImmutable<T>(this IArrayBuilderBlock<T> currentBlock) {
        return ImmutableArray.Create(currentBlock.Build());
    }
}

internal record ArrayBuilderElement<T>(int Length, IArrayBuilderBlock<T> Parent, T Element) : IArrayBuilderBlock<T>;

internal record ArrayBuilderCollection<T>
    (int Length, IArrayBuilderBlock<T> Parent, IReadOnlyCollection<T> Collection) : IArrayBuilderBlock<T>;