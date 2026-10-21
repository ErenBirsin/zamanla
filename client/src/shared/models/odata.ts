export interface ODataResponse<T> {
  value: T[]
  // yalnızca $count=true istenince gelir
  '@odata.count'?: number
}
