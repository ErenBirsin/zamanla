export interface Result<T> {
  data: T | null
  errorMessages: string[] | null
  isSuccessful: boolean
  statusCode: number
}
