export function toUtcDateTime(num: string): number {
    const date = new Date(num);
    const utcDate = Date.UTC(
        date.getFullYear(),
        date.getMonth(),
        date.getDate(),
        date.getHours(),
        date.getMinutes(),
        date.getSeconds()
    );
    return utcDate;
}