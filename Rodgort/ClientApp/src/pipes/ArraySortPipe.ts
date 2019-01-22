// https://stackoverflow.com/a/44511274/563532
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'sort'
})
export class ArraySortPipe implements PipeTransform {
  transform(array: any, fields: string): any[] {
    const fieldsArr = fields.split(',');
    if (!Array.isArray(array)) {
      return;
    }
    array.sort((a: any, b: any) => {
      let fieldIndex = 0;
      let sortResult = 0;
      do {
        const field = fieldsArr[fieldIndex++];
        sortResult = this.sort(a, b, field);
      } while (sortResult === 0 && fieldIndex < fieldsArr.length);
      return sortResult;
    });

    return array;
  }

  sort(a: any, b: any, field: string) {
    if (a[field] < b[field]) {
      return -1;
    } else if (a[field] > b[field]) {
      return 1;
    } else {
      return 0;
    }
  }
}
