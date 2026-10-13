// OData listeleri { "@odata.context": ..., "value": [...] } şeklinde döner
export interface ODataResponse<T> {
  value: T[]
}
