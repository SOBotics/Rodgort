// https://stackoverflow.com/a/44511274/563532
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'sortAscending'
})
export class ArraySortAscendingPipe  implements PipeTransform {
  transform(array: any, field: string): any[] {
    if (!Array.isArray(array)) {
      return;
    }
    array.sort((a: any, b: any) => {
      if (a[field] < b[field]) {
        return 1;
      } else if (a[field] > b[field]) {
        return -1;
      } else {
        return 0;
      }
    });

    return array;
  }
}
