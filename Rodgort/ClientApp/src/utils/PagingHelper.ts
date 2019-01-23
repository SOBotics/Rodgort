export interface PagingInfo {
    Name: string;
    Number: number;
    Active: boolean;
    Disabled: boolean;
    MinifiedShown: boolean;
    MinifiedHidden: boolean;
}

export function GetPagingInfo(pagedData: { pageNumber?: number, totalPages?: number }, numExtraPages = 2): PagingInfo[] {
    if (!pagedData) {
        return [];
    }
    const totalPages = pagedData.totalPages || 0;
    if (totalPages === 0) {
        return [];
    }
    const pageNumber = pagedData.pageNumber || 1;
    const pages = [];

    // Always add in 'Previous' button. Disabled if we're on page one (or less, for defensive coding)
    pages.push({
        Name: 'Previous',
        Number: pageNumber - 1,
        Active: false,
        Disabled: pageNumber <= 1
    });

    if (pageNumber > 1) {
        pages.push({
            Name: '1',
            Number: 1,
            Active: false,
            Disabled: false
        });
    }

    // Add in dots if we have a missing number between 1 and our first showing number
    if (pageNumber - numExtraPages > 2) {
        pages.push({
            Name: '...',
            Number: -1,
            Active: false,
            Disabled: true
        });
    } else if (pageNumber > 2) {
        pages.push({
            Name: '...',
            Number: -1,
            Active: false,
            MinifiedShown: true,
            Disabled: true
        });
    }

    // Add in pages before and after our selected page
    for (let i = pageNumber - numExtraPages; i <= pageNumber + numExtraPages; i++) {
        if (i <= 0 || i > totalPages) {
            continue;
        }
        if (pageNumber !== i && (i  === 1 || i === totalPages)) {
            continue;
        }
        pages.push({ Name: i + '', Number: i, Active: i === pageNumber, MinifiedHidden: i !== pageNumber });
    }

    if (totalPages - pageNumber - numExtraPages > 1) {
        pages.push({
            Name: '...',
            Number: -1,
            Active: false,
            Disabled: true
        });
    } else if (totalPages - pageNumber > 1) {
        pages.push({
            Name: '...',
            Number: -1,
            Active: false,
            MinifiedShown: true,
            Disabled: true
        });
    }

    if (totalPages - pageNumber > 0) {
        pages.push({
            Name: totalPages + '',
            Number: totalPages,
            Active: false,
            Disabled: false
        });
    }

    // Always add in 'Next' button. Disabled if we're on the last page (or above)
    pages.push({
        Name: 'Next',
        Number: pageNumber + 1,
        Active: false,
        Disabled: pageNumber >= totalPages
    });

    return pages;
}
