[<AutoOpen>]
module NinjaFs.TestLib.Library

open FSharp.Quotations
open FSharp.Quotations.Patterns

let private (|NewAnonRecord|_|) (expr: Expr) : option<Map<string, Expr>> =
    match expr with
    | NewRecord (ty, fields) when ty.Name.StartsWith "<>f__AnonymousType" ->
        ty.GetProperties()
        |> Array.indexed
        |> Array.map (fun (i, property) -> (property.Name, fields[i]))
        |> Map.ofArray
        |> Some
    | _ -> None

type Expr with
    static member ToIExpr(target: Expr) : Ast.Expr.IExpr =
        match target with
        | Call (object, method, args) ->
            match object with
            | Some (ValueWithName (_value, _ty, "builder@")) ->
                let target = Expr.ToIExpr args[0]

                match target with
                | :? Ast.BuilderMethodCall.BuilderMethodCall as (Ast.BuilderMethodCall.BuilderMethodCall payload) ->
                    builderMethodCall
                        {| Target = payload.Target
                           Chain =
                            payload.Chain
                            @ [ {| Method = method.Name
                                   Args = args[1..] |> List.map Expr.ToIExpr |} ] |}
                | _ ->
                    builderMethodCall
                        {| Target = target
                           Chain =
                            [ {| Method = method.Name
                                 Args = args[1..] |> List.map Expr.ToIExpr |} ] |}

            | Some object ->
                let object = Expr.ToIExpr object
                let args = args |> List.map Expr.ToIExpr

                methodCall
                    {| Object = object
                       Method = method.Name
                       Args = args |}
            | None ->
                let args = args |> List.map Expr.ToIExpr

                let func =
                    $"%s{method.DeclaringType.Namespace}.%s{method.DeclaringType.Name}.%s{method.Name}"

                functionCall {| Func = func; Args = args |}
        | Lambda (var, body) ->
            let body = Expr.ToIExpr body
            lambda {| VarName = var.Name; Body = body |}
        | NewAnonRecord fields ->
            let fields =
                fields
                |> Map.map (fun _ field -> Expr.ToIExpr field)

            newAnonRecord fields
        | NewUnionCase (unionCaseInfo, fields) ->
            let fields = fields |> List.map Expr.ToIExpr

            newUnionCase
                {| Name = unionCaseInfo.Name
                   Fields = fields |}
        | Var var -> varRef var.Name
        | Value (obj, ty) when obj = () && ty = typeof<unit> -> unitLiteral
        | Value (obj, ty) when ty = typeof<string> -> stringLiteral (obj :?> string)
        | _ -> failwith $"Unsupported expression: %A{target}"

open Thoth.Json.Net

let writeSnapshot (filename: string) (expr: Expr) =
    let content = Encode.toString 2 ((Expr.ToIExpr expr).Encoder())

    System.IO.File.WriteAllText(filename, $"{content}\n")
