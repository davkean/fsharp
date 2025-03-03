module Internal.Utilities.Library.Block

open System.Collections.Immutable

[<RequireQualifiedAccess>]
module ImmutableArrayBuilder =

    let create size : ImmutableArray<'T>.Builder = ImmutableArray.CreateBuilder(size)

[<RequireQualifiedAccess>]
module ImmutableArray =

    [<GeneralizableValue>]
    let empty<'T> = ImmutableArray<'T>.Empty

    let init n (f: int -> 'T) : ImmutableArray<_> =
        match n with
        | 0 -> ImmutableArray.Empty
        | 1 -> ImmutableArray.Create(f 0)
        | n ->
            if n < 0 then invalidArg "n" "Below zero."

            let builder = ImmutableArray.CreateBuilder(n)

            for i = 0 to n - 1 do
                builder.Add(f i)

            builder.MoveToImmutable()

    let iter f (arr: ImmutableArray<'T>) =
        for i = 0 to arr.Length - 1 do
            f arr[i]

    let iteri f (arr: ImmutableArray<'T>) =
        for i = 0 to arr.Length - 1 do
            f i arr[i]

    let iter2 f (arr1: ImmutableArray<'T1>) (arr2: ImmutableArray<'T2>) =
        if arr1.Length <> arr2.Length then
            invalidOp "Block lengths do not match."

        for i = 0 to arr1.Length - 1 do
            f arr1[i] arr2[i]

    let iteri2 f (arr1: ImmutableArray<'T1>) (arr2: ImmutableArray<'T2>) =
        if arr1.Length <> arr2.Length then
            invalidOp "Block lengths do not match."

        for i = 0 to arr1.Length - 1 do
            f i arr1[i] arr2[i]

    let map (mapper: 'T -> 'U) (arr: ImmutableArray<'T>) : ImmutableArray<_> =
        match arr.Length with
        | 0 -> ImmutableArray.Empty
        | 1 -> ImmutableArray.Create(mapper arr[0])
        | _ ->
            let builder = ImmutableArray.CreateBuilder(arr.Length)

            for i = 0 to arr.Length - 1 do
                builder.Add(mapper arr[i])

            builder.MoveToImmutable()

    let mapi (mapper: int -> 'T -> 'U) (arr: ImmutableArray<'T>) : ImmutableArray<_> =
        match arr.Length with
        | 0 -> ImmutableArray.Empty
        | 1 -> ImmutableArray.Create(mapper 0 arr[0])
        | _ ->
            let builder = ImmutableArray.CreateBuilder(arr.Length)

            for i = 0 to arr.Length - 1 do
                builder.Add(mapper i arr[i])

            builder.MoveToImmutable()

    let map2 (mapper: 'T1 -> 'T2 -> 'T) (arr1: ImmutableArray<'T1>) (arr2: ImmutableArray<'T2>) : ImmutableArray<_> =
        if arr1.Length <> arr2.Length then
            invalidOp "Block lengths do not match."

        match arr1.Length with
        | 0 -> ImmutableArray.Empty
        | 1 -> ImmutableArray.Create(mapper arr1[0] arr2[0])
        | n ->
            let builder = ImmutableArray.CreateBuilder(n)

            for i = 0 to n - 1 do
                builder.Add(mapper arr1[i] arr2[i])

            builder.MoveToImmutable()

    let mapi2 (mapper: int -> 'T1 -> 'T2 -> 'T) (arr1: ImmutableArray<'T1>) (arr2: ImmutableArray<'T2>) : ImmutableArray<_> =
        if arr1.Length <> arr2.Length then
            invalidOp "Block lengths do not match."

        match arr1.Length with
        | 0 -> ImmutableArray.Empty
        | 1 -> ImmutableArray.Create(mapper 0 arr1[0] arr2[0])
        | n ->
            let builder = ImmutableArray.CreateBuilder(n)

            for i = 0 to n - 1 do
                builder.Add(mapper i arr1[i] arr2[i])

            builder.MoveToImmutable()

    let concat (arrs: ImmutableArray<ImmutableArray<'T>>) : ImmutableArray<'T> =
        match arrs.Length with
        | 0 -> ImmutableArray.Empty
        | 1 -> arrs[0]
        | 2 -> arrs[ 0 ].AddRange(arrs[1])
        | _ ->
            let mutable acc = 0

            for h in arrs do
                acc <- acc + h.Length

            let builder = ImmutableArray.CreateBuilder(acc)

            for i = 0 to arrs.Length - 1 do
                builder.AddRange(arrs[i])

            builder.MoveToImmutable()

    let forall predicate (arr: ImmutableArray<'T>) =
        let len = arr.Length

        let rec loop i =
            i >= len || (predicate arr[i] && loop (i + 1))

        loop 0

    let forall2 predicate (arr1: ImmutableArray<'T1>) (arr2: ImmutableArray<'T2>) =
        if arr1.Length <> arr2.Length then
            invalidOp "Block lengths do not match."

        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt (predicate)
        let len1 = arr1.Length

        let rec loop i =
            i >= len1 || (f.Invoke(arr1[i], arr2[i]) && loop (i + 1))

        loop 0

    let tryFind predicate (arr: ImmutableArray<'T>) =
        let rec loop i =
            if i >= arr.Length then None
            else if predicate arr[i] then Some arr[i]
            else loop (i + 1)

        loop 0

    let tryFindIndex predicate (arr: ImmutableArray<'T>) =
        let len = arr.Length

        let rec go n =
            if n >= len then None
            elif predicate arr[n] then Some n
            else go (n + 1)

        go 0

    let tryPick chooser (arr: ImmutableArray<'T>) =
        let rec loop i =
            if i >= arr.Length then
                None
            else
                match chooser arr[i] with
                | None -> loop (i + 1)
                | res -> res

        loop 0

    let ofSeq (xs: 'T seq) = ImmutableArray.CreateRange(xs)

    let append (arr1: ImmutableArray<'T1>) (arr2: ImmutableArray<'T1>) : ImmutableArray<_> = arr1.AddRange(arr2)

    let createOne (item: 'T) : ImmutableArray<_> = ImmutableArray.Create(item)

    let filter predicate (arr: ImmutableArray<'T>) : ImmutableArray<'T> =
        let builder = ImmutableArray.CreateBuilder(arr.Length)

        for i = 0 to arr.Length - 1 do
            if predicate arr[i] then builder.Add(arr[i])

        builder.Capacity <- builder.Count
        builder.MoveToImmutable()

    let exists predicate (arr: ImmutableArray<'T>) =
        let len = arr.Length

        let rec loop i =
            i < len && (predicate arr[i] || loop (i + 1))

        len > 0 && loop 0

    let choose (chooser: 'T -> 'U option) (arr: ImmutableArray<'T>) : ImmutableArray<'U> =
        let builder = ImmutableArray.CreateBuilder(arr.Length)

        for i = 0 to arr.Length - 1 do
            let result = chooser arr[i]

            if result.IsSome then builder.Add(result.Value)

        builder.Capacity <- builder.Count
        builder.MoveToImmutable()

    let isEmpty (arr: ImmutableArray<_>) = arr.IsEmpty

    let fold folder state (arr: ImmutableArray<_>) =
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt (folder)
        let mutable state = state

        for i = 0 to arr.Length - 1 do
            state <- f.Invoke(state, arr[i])

        state
