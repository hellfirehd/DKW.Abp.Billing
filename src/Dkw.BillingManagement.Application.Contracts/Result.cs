// DKW Billing Management
// Copyright (C) 2025 Doug Wilson
//
// This program is free software: you can redistribute it and/or modify it under the terms of
// the GNU Affero General Public License as published by the Free Software Foundation, either
// version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along with this
// program. If not, see <https://www.gnu.org/licenses/>.

namespace Dkw.BillingManagement;

public class Result
{
    public Boolean IsSuccess { get; set; } = true;
    public String Message { get; set; } = String.Empty;
    public Exception? Exception { get; set; }

    public static Result Success() => new();

    public static Result<T> Success<T>(T value)
        => new() { IsSuccess = true, Value = value };

    public static Result Failure(String message, Exception? exception = null)
    {
        return new Result()
        {
            IsSuccess = false,
            Message = message,
            Exception = exception
        };
    }
}

public class Result<T> : Result
{
    public T Value { get; set; } = default!;
}

